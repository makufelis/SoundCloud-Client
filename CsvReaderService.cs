using System.Collections.Generic;
using System.IO;
using BoldReports.Data.Csv;
using SoundCloudPlayer.Models;

namespace SoundCloudPlayer.Services
{
    public class CsvReaderService
    {
        public List<SoundCloudTrack> ReadCsv(string filePath)
        {
            var tracks = new List<SoundCloudTrack>();

            using (var csvReader = new CsvReader(new StreamReader(filePath), true))
            {
                while (csvReader.Read())
                {
                    var track = new SoundCloudTrack
                    {
                        Url = csvReader.GetString(0),
                        Name = csvReader.GetString(1)
                    };
                    tracks.Add(track);
                }
            }

            return tracks;
        }
    }
}
