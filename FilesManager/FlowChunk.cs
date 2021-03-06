﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SOCISA
{
    public enum PostChunkStatus
    {
        Error = 0,
        Done = 1,
        PartlyDone = 2
    }
    public class FlowJsPostChunkResponse
    {
        public FlowJsPostChunkResponse()
        {
            ErrorMessages = new List<string>();
        }

        public string Path { get; set; }
        public string OriginalFileName { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public long Size { get; set; }
        public string CreationDate { get; set; }
        public PostChunkStatus Status { get; set; }
        public List<string> ErrorMessages { get; set; }
    }

    public class FlowChunk
    {
        public int Number { get; set; }
        public long Size { get; set; }
        public long TotalSize { get; set; }
        public string Identifier { get; set; }
        public string FileName { get; set; }
        public int TotalChunks { get; set; }

        internal bool ParseForm(Dictionary<string, object> form)
        {
            try
            {
                if (string.IsNullOrEmpty(form["flowIdentifier"].ToString()) || string.IsNullOrEmpty(form["flowFilename"].ToString()))
                    return false;

                Number = int.Parse(form["flowChunkNumber"].ToString());
                Size = long.Parse(form["flowChunkSize"].ToString());
                TotalSize = long.Parse(form["flowTotalSize"].ToString());
                Identifier = CleanIdentifier(form["flowIdentifier"].ToString());
                FileName = form["flowFilename"].ToString();
                TotalChunks = int.Parse(form["flowTotalChunks"].ToString());
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal bool ValidateBusinessRules(FlowValidationRules rules, out List<string> errorMessages)
        {
            var acceptedFileExtensions = new AcceptedFileExtensions();

            errorMessages = new List<string>();
            if (rules.MaxFileSize.HasValue && TotalSize > rules.MaxFileSize.Value)
                errorMessages.Add(rules.MaxFileSizeMessage ?? "size");

            if (!acceptedFileExtensions.IsExtensionAllowed(rules.AcceptedExtensions, FileName))
            {
                errorMessages.Add(rules.AcceptedExtensionsMessage ?? "type");
            }

            return errorMessages.Count == 0;
        }

        private string CleanIdentifier(string identifier)
        {
            identifier = Regex.Replace(identifier, "/[^0-9A-Za-z_-]/g", "");
            return identifier;
        }
    }

    public class FlowValidationRules
    {
        public FlowValidationRules()
        {
            AcceptedExtensions = new List<string>();
        }

        public long? MaxFileSize { get; set; }
        public string MaxFileSizeMessage { get; set; }

        public List<string> AcceptedExtensions { get; set; }
        public string AcceptedExtensionsMessage { get; set; }
    }

}
