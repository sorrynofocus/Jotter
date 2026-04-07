/*
* Jotter
* C.Winters / US / Arizona / ASUS ROG MAXIMUS Z790 DARK HERO  (GaleHarper)
*  2026
*  Purpose: Exporter.cs - Export notes from Jotter in Markdown format.
*/

using com.nobodynoze.notemanager;
using System;
using System.Globalization;
using System.IO;
using System.Text;

/*
    Jotter.Utils.Exporter

    Goal: Provide functionality to export notes from Jotter in Markdown format,
    allowing users to easily share or backup their notes.

    Support two export paths:

    *Single note export*
    Right-click a note
    Choose Export to Markdown
    Save that one note as a .md file

    *Bulk export*
    In Settings
    Click Export Notes
    Choose a folder
    Export every note as its own .md file
 */

namespace Jotter.Utils.Exporter
{
    public static class NoteMarkdownExporter
    {
        /// <summary>
        /// Exports a single note to a Markdown file at the specified file path. The Markdown 
        /// file will include the note's title, creation date, and body text formatted appropriately.
        /// </summary>
        /// <param name="note">The note to export.</param>
        /// <param name="filePath">The file path where the Markdown file will be saved.</param>
        /// <exception cref="ArgumentNullException">Thrown if the note is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the file path is null or empty.</exception>
        /// NOTE: This is used by func ExportNotesToMarkdownFolder()
        public static void ExportNoteToMarkdownFile(Note note, string filePath)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));
    
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            // Build the Markdown content from the note's properties
            string markdown = BuildMarkdownFromNote(note);

            string? directory = Path.GetDirectoryName(filePath);
            
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(filePath, markdown, Encoding.UTF8);
        }

        /// <summary>
        /// Exports a collection of notes to individual Markdown files within the specified 
        /// folder. Each note will be saved as a separate .md file, with the file name derived 
        /// from the note's title. If multiple notes have the same title, unique file names 
        /// will be generated to avoid overwriting existing files.
        /// </summary>
        /// <param name="notes">The collection of notes to export.</param>
        /// <param name="folderPath">The folder path where the Markdown files will be saved.</param>
        /// <returns>The number of notes successfully exported.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the notes collection is null.</exception>
        /// <exception cref="ArgumentException"></exception>
        public static int ExportNotesToMarkdownFolder(IEnumerable<Note> notes, string folderPath)
        {
            if (notes == null)
                throw new ArgumentNullException(nameof(notes));

            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            int exportCount = 0;

            foreach (Note note in notes)
            {
                if (note == null)
                    continue;

                string defaultFileName = GetDefaultMarkdownFileName(note);
                string targetFilePath = Path.Combine(folderPath, defaultFileName);
                string uniqueFilePath = GetUniqueFilePath(targetFilePath);

                // Export the note to the determined unique file path
                ExportNoteToMarkdownFile(note, uniqueFilePath);
                exportCount++;
            }

            return exportCount;
        }

        /// <summary>
        /// Constructs a Markdown-formatted string representation of a note, including the title as a header,
        /// creation date, and body content.
        /// </summary>
        /// <param name="note">The note to convert to Markdown.</param>
        /// <returns>A Markdown-formatted string representing the note.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the note is null.</exception>
        public static string BuildMarkdownFromNote(Note note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            string title = string.IsNullOrWhiteSpace(note.Title)
                            ? "Untitled Note"
                            : note.Title.Trim();

            // Normalize line endings in the body text and trim any trailing whitespace
            // This is the heart of the content - it's the note's text, but we want to
            // ensure it has consistent line endings and no trailing whitespace
            string body = note.Text ?? string.Empty;
            body = NormalizeLineEndings(body).TrimEnd();

            string createdText = note.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);

            var sb = new StringBuilder();

            sb.Append("# ");
            sb.AppendLine(title);
            sb.AppendLine();
            sb.Append("Created: ");
            sb.AppendLine(createdText);
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(body))
            {
                sb.Append(body);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        // Generates a default file name for a note based on its title, ensuring it is valid for the file system.
        public static string GetDefaultMarkdownFileName(Note note)
        {
            if (note == null)
                throw new ArgumentNullException(nameof(note));

            string title = string.IsNullOrWhiteSpace(note.Title)
                            ? "Untitled Note"
                            : note.Title.Trim();

            return SanitizeFileName(title) + ".md";
        }

        /// <summary>
        /// Normalizes line endings in the given text to ensure consistent formatting across different platforms.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string NormalizeLineEndings(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Replaces invalid file name characters with underscores and trims trailing dots and spaces.
        /// </summary>
        /// <param name="fileName">The file name to sanitize.</param>
        /// <returns>A sanitized file name safe for use in the file system.</returns>
        private static string SanitizeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder();

            foreach (char c in fileName)
            {
                bool invalid = false;

                for (int i = 0; i < invalidChars.Length; i++)
                {
                    if (c == invalidChars[i])
                    {
                        invalid = true;
                        break;
                    }
                }

                sb.Append(invalid ? '_' : c);
            }

            string result = sb.ToString().Trim().TrimEnd('.', ' ');

            if (string.IsNullOrWhiteSpace(result))
                result = "Untitled Note";

            return result;
        }

        /// <summary>
        /// Generates a unique file path by appending a numeric suffix if a file with the same name already exists.
        /// </summary>
        /// <param name="filePath">The initial file path.</param>
        /// <returns>A unique file path that does not conflict with existing files.</returns>
        private static string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            int duplicateCounter = 2;

            // Loop to find a unique file name by appending a counter in parentheses
            while (true)
            {
                string candidateFileName = $"{fileNameWithoutExtension} ({duplicateCounter}){extension}";
                string candidatePath = Path.Combine(directory, candidateFileName);

                if (!File.Exists(candidatePath))
                    return candidatePath;

                duplicateCounter++;
            }
        }
    }
}
