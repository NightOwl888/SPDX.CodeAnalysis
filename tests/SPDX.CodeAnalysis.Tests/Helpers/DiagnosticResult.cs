using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SPDX.CodeAnalysis.Tests
{
    /// <summary>
    /// Location where the diagnostic appears, as determined by path, line number, and column number.
    /// </summary>
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Used for testing")]
    public struct DiagnosticResultLocation
    {
        public DiagnosticResultLocation(string path, int line, int column)
        {
            if (line < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");
            }

            if (column < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
            }

            this.Path = path;
            this.Line = line;
            this.Column = column;
        }

        public string Path { get; }
        public int Line { get; }
        public int Column { get; }
    }

    /// <summary>
    /// Struct that stores information about a Diagnostic appearing in a source
    /// </summary>
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Used for testing")]
    public struct DiagnosticResult
    {
        private DiagnosticResultLocation[] locations;

        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Used for testing")]
        public DiagnosticResultLocation[] Locations
        {
            get
            {
                if (this.locations == null)
                {
                    this.locations = Array.Empty<DiagnosticResultLocation>();
                }
                return this.locations;
            }
            set => this.locations = value;
        }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; set; }

        public string Message { get; set; }

        public string Path => this.Locations.Length > 0 ? this.Locations[0].Path : "";

        public int Line => this.Locations.Length > 0 ? this.Locations[0].Line : -1;

        public int Column => this.Locations.Length > 0 ? this.Locations[0].Column : -1;
    }
}
