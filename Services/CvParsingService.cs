using ATSProject.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xceed.Words.NET;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ATSProject.Services
{
    // Simple structured result to inspect before applying to DB
    public class ParsedCvResult
    {
        public string RawText { get; set; } = string.Empty;
        public List<string> Emails { get; set; } = new();
        public List<string> Phones { get; set; } = new();
        public string Name { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public List<ExperienceEntry> Experience { get; set; } = new();
        public List<EducationEntry> Education { get; set; } = new();
        public Dictionary<string, List<string>> Sections { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public class ExperienceEntry
    {
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string DateRange { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class EducationEntry
    {
        public string Institution { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string DateRange { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CvParsingService
    {
        // keywords for headings. Add more synonyms if you see variants.
        private readonly string[] skillsHeadings = new[] {
            "skills","technical skills","expertise","core competencies","skillset","areas of expertise","technical summary"
        };

        private readonly string[] experienceHeadings = new[] {
            "work experience","professional experience","experience","employment history","career history","work history","roles","professional background"
        };

        private readonly string[] educationHeadings = new[] {
            "education","academic background","qualifications","education & training","academic qualifications","education history"
        };

        private readonly string[] certificationHeadings = new[] {
            "certifications","certificates","licenses"
        };

        private readonly string[] projectHeadings = new[] {
            "projects","selected projects","project experience"
        };

        private readonly string[] otherHeadings = new[] {
            "languages","interests","summary","profile","about","objective","personal profile","achievements"
        };

        // Public backward-compatible method (keeps your existing call sites)
        public void ParseCandidateCv(Candidate candidate, string cvFullPath)
        {
            var parsed = Parse(cvFullPath, saveRawJson: true);
            if (parsed != null)
                ApplyParsedToCandidate(candidate, parsed);
        }

        // Main parse: returns structured parsed result and optionally saves JSON file
        public ParsedCvResult? Parse(string cvFullPath, bool saveRawJson = true)
        {
            if (string.IsNullOrEmpty(cvFullPath) || !File.Exists(cvFullPath))
                return null;

            string raw = ExtractTextFromFile(cvFullPath);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            // normalize but KEEP meaningful line breaks and paragraph separation
            string normalized = NormalizeKeepLines(raw);

            var parsed = new ParsedCvResult
            {
                RawText = normalized
            };

            // split into lines and preserve them (trim each line)
            var lines = normalized.Split('\n')
                                  .Select(l => l.Trim())
                                  .Where(l => !string.IsNullOrWhiteSpace(l))
                                  .ToList();

            // SECTIONIZE: split document into named sections (top/default + headings)
            parsed.Sections = Sectionize(lines);

            // Extract emails & phones from full text
            parsed.Emails = ExtractEmails(normalized).ToList();
            parsed.Phones = ExtractPhones(normalized).ToList();

            // Name: try explicit label, then top-of-doc heuristics, then email fallback
            parsed.Name = ExtractName(parsed.Sections, parsed.Emails);

            // Skills: from skills section or fallback to inline 'Skills:' occurrences
            parsed.Skills = ExtractSkills(parsed.Sections, normalized).ToList();

            // Experience: parse experience section with heuristics for date-lines + role/company
            if (parsed.Sections.TryGetValue("experience", out var expLines))
            {
                parsed.Experience = ParseExperienceBlock(string.Join("\n", expLines));
            }
            else
            {
                // fallback: search for any heading-like combination containing experience
                var allExpText = TryJoinSections(parsed.Sections, experienceHeadings);
                if (!string.IsNullOrEmpty(allExpText))
                    parsed.Experience = ParseExperienceBlock(allExpText);
            }

            // Education: parse education block
            if (parsed.Sections.TryGetValue("education", out var eduLines))
            {
                parsed.Education = ParseEducationBlock(string.Join("\n", eduLines));
            }
            else
            {
                var allEduText = TryJoinSections(parsed.Sections, educationHeadings);
                if (!string.IsNullOrEmpty(allEduText))
                    parsed.Education = ParseEducationBlock(allEduText);
            }

            // Save raw JSON next to CV for manual inspection (useful for tuning)
            if (saveRawJson)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(parsed, options);
                    string jsonPath = System.IO.Path.ChangeExtension(cvFullPath, ".parsed.json");
                    File.WriteAllText(jsonPath, json);
                }
                catch
                {
                    // ignore write errors; parsing still succeeds
                }
            }

            return parsed;
        }

        // Map structured parsed object into your Candidate fields (simple join rules)
        public void ApplyParsedToCandidate(Candidate candidate, ParsedCvResult parsed)
        {
            // Overwrite candidate contact info with CV values (prefer CV info)
            if (parsed.Emails?.FirstOrDefault() is string cvEmail && !string.IsNullOrWhiteSpace(cvEmail))
                candidate.Email = cvEmail;

            if (parsed.Phones?.FirstOrDefault() is string cvPhone && !string.IsNullOrWhiteSpace(cvPhone))
                candidate.Phone = FormatPhoneDigitsOnly(cvPhone);

            // Name
            if (!string.IsNullOrWhiteSpace(parsed.Name))
                candidate.Name = parsed.Name;

            // Skills -> join to single string (comma separated)
            if (parsed.Skills?.Any() == true)
                candidate.Skills = string.Join(", ", parsed.Skills.Select(s => s.Trim()).Where(s => s.Length > 0));

            // Experience -> convert structured experience entries into a human readable string
            if (parsed.Experience?.Any() == true)
            {
                candidate.Experience = string.Join(" | ",
                    parsed.Experience.Select(e =>
                    {
                        var title = string.IsNullOrWhiteSpace(e.Title) ? "" : e.Title.Trim();
                        var company = string.IsNullOrWhiteSpace(e.Company) ? "" : $" at {e.Company.Trim()}";
                        var dates = string.IsNullOrWhiteSpace(e.DateRange) ? "" : $" ({e.DateRange})";
                        return $"{title}{company}{dates}".Trim();
                    }).Where(s => s.Length > 0));
            }

            // Education -> join
            if (parsed.Education?.Any() == true)
            {
                candidate.Education = string.Join(" | ",
                    parsed.Education.Select(e =>
                    {
                        var deg = string.IsNullOrWhiteSpace(e.Degree) ? e.Institution : e.Degree;
                        var dates = string.IsNullOrWhiteSpace(e.DateRange) ? "" : $" ({e.DateRange})";
                        return $"{deg}{dates}".Trim();
                    }).Where(s => s.Length > 0));
            }
        }

        #region Helper parsing functions

        private static string TryJoinSections(Dictionary<string, List<string>> sections, string[] headings)
        {
            foreach (var h in headings)
            {
                if (sections.TryGetValue(h.ToLower(), out var lines) && lines.Any())
                    return string.Join("\n", lines);
            }
            return string.Empty;
        }

        private Dictionary<string, List<string>> Sectionize(List<string> lines)
        {
            var headingToKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            void addHeadings(string[] keys, string keyName)
            {
                foreach (var h in keys) headingToKey[h.ToLower()] = keyName;
            }

            addHeadings(skillsHeadings, "skills");
            addHeadings(experienceHeadings, "experience");
            addHeadings(educationHeadings, "education");
            addHeadings(certificationHeadings, "certifications");
            addHeadings(projectHeadings, "projects");
            addHeadings(otherHeadings, "other");

            var sections = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            string current = "top";
            sections[current] = new List<string>();

            foreach (var line in lines)
            {
                var lowered = line.ToLowerInvariant();

                // check if the line contains any heading phrase
                var matched = headingToKey.Keys.FirstOrDefault(k => lowered.Contains(k));
                if (matched != null)
                {
                    current = headingToKey[matched];
                    if (!sections.ContainsKey(current))
                        sections[current] = new List<string>();
                    continue; // skip the heading line itself
                }

                if (!sections.ContainsKey(current))
                    sections[current] = new List<string>();

                sections[current].Add(line);
            }

            return sections;
        }

        private static IEnumerable<string> ExtractEmails(string text)
        {
            var matches = Regex.Matches(text, @"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[A-Za-z]{2,}", RegexOptions.IgnoreCase);
            return matches.Cast<Match>().Select(m => m.Value).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> ExtractPhones(string text)
        {
            // loose phone matcher, returns raw matches that include separators
            var matches = Regex.Matches(text, @"((?:\+?\d{1,3}[\s\-\._\(]*)?(?:\(?\d{2,4}\)?[\s\-\._]*)?(?:\d[\d\-\s\._\(\)]{5,}\d))");
            foreach (Match m in matches)
            {
                string raw = m.Value;
                string digits = Regex.Replace(raw, @"\D", "");
                if (digits.Length >= 7 && digits.Length <= 15)
                    yield return raw.Trim();
            }
        }

        private static string ExtractName(Dictionary<string, List<string>> sections, List<string> emails)
        {
            // 1) check for explicit "Name:" in top section
            if (sections.TryGetValue("top", out var topLines))
            {
                foreach (var line in topLines.Take(10))
                {
                    var m = Regex.Match(line, @"\bName\b\s*[:\-]\s*(.+)", RegexOptions.IgnoreCase);
                    if (m.Success)
                        return CleanName(m.Groups[1].Value);
                }

                // 2) top-of-doc heuristics: first few lines NOT containing email/phone/linkedin and that look like a human name
                foreach (var line in topLines.Take(8))
                {
                    string low = line.ToLowerInvariant();
                    if (low.Contains("resume") || low.Contains("curriculum vitae") || low.Contains("cv") || low.Contains("profile") || low.Contains("objective"))
                        continue;
                    if (line.Contains("@") || Regex.IsMatch(line, @"\d")) // skip lines with digits/emails
                        continue;
                    // typical name is 2–4 words, each starting with a letter
                    var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length >= 2 && words.Length <= 4 && words.All(w => Regex.IsMatch(w, @"^[A-Za-z][A-Za-z\.'\-]*$")))
                        return CleanName(line);
                }
            }

            // 3) fallback: from email local part
            var email = emails?.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(email))
            {
                var local = email.Split('@')[0];
                var parts = Regex.Split(local, @"[\._\-\+]+").Where(p => p.Length > 1).Select(p => ToTitleCase(p)).ToArray();
                if (parts.Length >= 2)
                    return string.Join(" ", parts);
                if (parts.Length == 1)
                    return parts[0];
            }

            return string.Empty;
        }

        private static string ToTitleCase(string s)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());
        }

        private static string CleanName(string s)
        {
            s = Regex.Replace(s, @"\s{2,}", " ").Trim();
            if (s == s.ToUpperInvariant())
                s = ToTitleCase(s);
            return s;
        }

        private static IEnumerable<string> ExtractSkills(Dictionary<string, List<string>> sections, string fullText)
        {
            if (sections.TryGetValue("skills", out var skillsLines) && skillsLines.Any())
            {
                foreach (var line in skillsLines)
                {
                    // Stop if line looks like a heading of another section
                    if (Regex.IsMatch(line, @"\b(Experience|Education|Certifications|Projects)\b", RegexOptions.IgnoreCase))
                        yield break;

                    // Skip job titles/years
                    if (Regex.IsMatch(line, @"\b(19|20)\d{2}\b") ||
                        Regex.IsMatch(line, @"\bPresent\b", RegexOptions.IgnoreCase) ||
                        Regex.IsMatch(line, @"\b(Manager|Director|Engineer|Officer|Developer|Intern|Consultant|Coordinator|Lead|Specialist)\b", RegexOptions.IgnoreCase))
                        continue;

                    foreach (var s in SplitSkillLine(line))
                        yield return s;
                }
                yield break;
            }

            // fallback inline detection
            var m = Regex.Match(fullText,
                @"(?:Skills|Expertise|Technical Skills)\s*[:\-]\s*(.+?)(?=(Education|Experience|Work|Projects|Certifications|$))",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (m.Success)
            {
                foreach (var s in SplitSkillLine(m.Groups[1].Value))
                    yield return s;
            }
        }


        private static IEnumerable<string> SplitSkillLine(string raw)
        {
            var parts = Regex.Split(raw, @"[•●\-\|\n,;·]+")
                            .Select(p => p.Trim())
                            .Where(p => p.Length > 1)
                            .Select(p => Regex.Replace(p, @"\s{2,}", " "));
            foreach (var p in parts.Distinct(StringComparer.OrdinalIgnoreCase))
                yield return p;
        }

        private List<ExperienceEntry> ParseExperienceBlock(string blockText)
        {
            var lines = blockText.Split('\n')
                                 .Select(l => l.Trim())
                                 .Where(l => l.Length > 0)
                                 .ToList();

            var result = new List<ExperienceEntry>();

            // === Pass 1: Date-line based detection ===
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                // If line looks like it has a year, month, or "Present"
                if (Regex.IsMatch(line, @"\b(19|20)\d{2}\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line, @"\bPresent\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line, @"\b(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b", RegexOptions.IgnoreCase))
                {
                    // Extract date substring
                    string dateRange = ExtractDateRangeFromLine(line);

                    // Try to get role/company (remove date from line)
                    string roleCompany = !string.IsNullOrEmpty(dateRange)
                        ? Regex.Replace(line, Regex.Escape(dateRange), "", RegexOptions.IgnoreCase).Trim()
                        : line.Trim();

                    if (string.IsNullOrEmpty(roleCompany))
                    {
                        // Look backwards for role/company info
                        int j = i - 1;
                        var accum = new List<string>();
                        while (j >= 0 && accum.Count < 3)
                        {
                            var cand = lines[j];
                            if (Regex.IsMatch(cand, @"\b(19|20)\d{2}\b", RegexOptions.IgnoreCase) ||
                                Regex.IsMatch(cand, @"\bPresent\b", RegexOptions.IgnoreCase))
                                break;
                            accum.Insert(0, cand);
                            j--;
                        }
                        roleCompany = string.Join(" | ", accum).Trim();
                    }

                    // Split role/company
                    string title = roleCompany;
                    string company = string.Empty;
                    if (!string.IsNullOrWhiteSpace(roleCompany))
                    {
                        var separators = new char[] { ',', '–', '—', '-' };
                        var split = roleCompany.Split(separators, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (split.Length >= 2)
                        {
                            title = split[0].Trim();
                            company = split[1].Trim();
                        }
                    }

                    // Add entry if new
                    if (!result.Any(r =>
                            r.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&
                            r.DateRange.Equals(dateRange, StringComparison.OrdinalIgnoreCase)))
                    {
                        result.Add(new ExperienceEntry
                        {
                            Title = title,
                            Company = company,
                            DateRange = string.IsNullOrWhiteSpace(dateRange) ? "" : dateRange,
                            Description = ""
                        });
                    }
                }
            }

            // === Pass 2: Inline "Role (Date)" patterns ===
            var inlineMatches = Regex.Matches(blockText,
                @"(?<role>[^()\n]{3,120})\s*[\(\[]\s*(?<dates>[^()\]\n]{3,80})\s*[\)\]]");

            foreach (Match m in inlineMatches)
            {
                string role = m.Groups["role"].Value.Trim();
                string dates = m.Groups["dates"].Value.Trim();

                if (!result.Any(r =>
                        r.Title.Equals(role, StringComparison.OrdinalIgnoreCase) &&
                        r.DateRange.Equals(dates, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(new ExperienceEntry
                    {
                        Title = role,
                        Company = "",
                        DateRange = dates,
                        Description = ""
                    });
                }
            }

            return result;
        }


        private List<EducationEntry> ParseEducationBlock(string blockText)
        {
            var lines = blockText.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
            var result = new List<EducationEntry>();

            foreach (var line in lines)
            {
                if (Regex.IsMatch(line, @"\b(Bachelor|Master|B\.Sc|M\.Sc|BS|MS|MBA|PhD|High School|Diploma)\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line, @"\b(University|College|School|Institute|Academy)\b", RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(line, @"\b(19|20)\d{2}\b", RegexOptions.IgnoreCase))
                {
                    string degree = "";
                    string institution = "";
                    string dates = ExtractDateRangeFromLine(line);

                    string noDates = line;
                    if (!string.IsNullOrEmpty(dates))
                        noDates = line.Replace(dates, "").Trim();

                    var parts = noDates.Split(',', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        degree = parts[0].Trim();
                        institution = parts[1].Trim();
                    }
                    else
                    {
                        var parts2 = Regex.Split(noDates, @"\s+[-–—]\s+");
                        if (parts2.Length == 2)
                        {
                            degree = parts2[0].Trim();
                            institution = parts2[1].Trim();
                        }
                        else
                        {
                            if (Regex.IsMatch(noDates, @"\b(University|College|School|Institute|Academy)\b", RegexOptions.IgnoreCase))
                                institution = noDates;
                            else
                                degree = noDates;
                        }
                    }

                    result.Add(new EducationEntry
                    {
                        Institution = institution,
                        Degree = degree,
                        DateRange = dates,
                        Description = ""
                    });
                }
            }
            return result;
        }



        // Extract a cleaned date-range-like substring from a line (year pairs or month-year or 'present')
        private static string ExtractDateRangeFromLine(string line)
        {
            // normalize underscores to spaces and normalized dashes
            var s = line.Replace('_', ' ').Replace("–", "-").Replace("—", "-");

            // try to find patterns like "March 21, 2021 - Present" or "2021 - 2022" or "Nov 2021 - Feb 2022"
            var rx = new Regex(@"\b((?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:tember)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)[^\d\n]{0,20}\d{4}|(?:19|20)\d{2})(?:\s*[-_–—to]{1,8}\s*(?:Present|present|(?:Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:tember)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)[^\d\n]{0,20}\d{4}|(?:19|20)\d{2}))?", RegexOptions.IgnoreCase);
            var m = rx.Match(s);
            if (m.Success)
            {
                var found = m.Value.Trim();
                // shorten spaces
                found = Regex.Replace(found, @"\s{2,}", " ");
                return found;
            }

            // fallback: any 4-digit year pair with optional text around it
            var m2 = Regex.Match(s, @"\b(19|20)\d{2}\b.*?\b(19|20)\d{2}\b", RegexOptions.Singleline);
            if (m2.Success)
                return m2.Value.Trim();

            // fallback: single year if present
            var m3 = Regex.Match(s, @"\b(19|20)\d{2}\b");
            if (m3.Success)
                return m3.Value.Trim();

            return string.Empty;
        }

        private static string NormalizeKeepLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
            // collapse more than 2 newlines into two (keeps paragraph separation)
            text = Regex.Replace(text, @"\n{3,}", "\n\n");
            // replace tabs with spaces
            text = text.Replace("\t", " ");
            // collapse multiple spaces inside lines
            var lines = text.Split('\n').Select(l => Regex.Replace(l, @"[ ]{2,}", " ").TrimEnd());
            return string.Join("\n", lines).Trim();
        }

        private static string ExtractTextFromFile(string filePath)
        {
            string ext = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
            if (ext == ".pdf")
            {
                using var reader = new PdfReader(filePath);
                var sb = new StringBuilder();
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    var page = PdfTextExtractor.GetTextFromPage(reader, i);
                    sb.AppendLine(page);
                }
                return sb.ToString();
            }
            else if (ext == ".docx")
            {
                using var doc = DocX.Load(filePath);
                return doc.Text;
            }
            else
            {
                // plain text or unsupported: return as-is
                return File.ReadAllText(filePath);
            }
        }

        private static string FormatPhoneDigitsOnly(string raw)
        {
            var digits = Regex.Replace(raw ?? "", @"\D", "");
            if (string.IsNullOrEmpty(digits)) return raw;
            // if local Pakistan mobile (starts with 03) keep as-is, else if starts with 92 add + if missing
            if (digits.StartsWith("0092")) digits = "+" + digits.Substring(2);
            else if (digits.StartsWith("92") && !digits.StartsWith("+92")) digits = "+" + digits;
            else if (digits.StartsWith("0") && digits.Length == 11) { /* keep as 0XXXXXXXXXX */ }
            return digits;
        }

        #endregion
    }
}
