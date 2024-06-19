using CsvHelper;
using CsvHelper.Configuration;
using SoundCloud.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SoundCloud.Services
{
    public class CsvReaderService
    {
        public List<SoundCloudTrack> ReadCsv(string filePath)
        {
            var tracks = new List<SoundCloudTrack>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false, // Indicate that there is no header
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<SoundCloudTrackMap>();
                tracks = csv.GetRecords<SoundCloudTrack>().ToList();
            }

            return tracks;
        }
    }

    public class SoundCloudTrackMap : ClassMap<SoundCloudTrack>
    {
        public SoundCloudTrackMap()
        {
            Map(m => m.Url).Index(0);
            Map(m => m.Name).Index(1);
        }
    }
}
