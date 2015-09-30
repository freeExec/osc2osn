using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Osc2Osn.OsmandExport
{
    [Serializable]
    [XmlRoot(ElementName = "osmChange")]
    public class OsmChange
    {
        [XmlArray("create")]
        public List<Note> notes;
    }
/*    [Serializable]
    [XmlRoot(ElementName = "osm-notes")]
    public class OsmNotes : List<Note>
    {
    }
*/
    [Serializable]
    [XmlType(TypeName = "note")]
    public class Note
    {
        [XmlAttribute]
        public double lat;
        [XmlAttribute]
        public double lon;
        [XmlAttribute]
        public long id;

        public Comment comment;
    }

    [Serializable]
    public class Comment
    {
        [XmlAttribute]
        public string text;
    }
}

namespace Osc2Osn.JosmImport
{
    [Serializable]
    [XmlRoot(ElementName = "osm-notes")]
    public class OsmNotes : List<Note>
    {
        //List<Note> notes;
    }

    [Serializable]
    [XmlType(TypeName = "note")]
    public class Note
    {
        [XmlAttribute]
        public double lat;
        [XmlAttribute]
        public double lon;
        [XmlAttribute]
        public long id;
        [XmlAttribute]
        public string created_at;

        [XmlElement]
        public Comment comment;
    }

    [Serializable]
    public class Comment
    {
        [XmlAttribute]
        public string action = "opened";
        [XmlAttribute]
        public string user;
        [XmlAttribute]
        public string timestamp;
        [XmlText]
        public string text;
    }
}

namespace Osc2Osn
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Converter Osmand note export to JOSM import note. Created by freeExec (C) 2015");
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Osc2Osn.exe poi_modification.osc [josm_import.osn] [-u=username] [--positiveID]");
                Console.ReadKey();
                return;
            }

            var date = DateTime.Now;
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Local);
            var dateStr = date.ToString("yyyy-MM-ddTHH:mm:sszz");            
            
            string filename = args[0];

            XmlSerializer serOsmand = new XmlSerializer(typeof(OsmandExport.OsmChange));
            StreamReader reader = new StreamReader(filename);
            OsmandExport.OsmChange notesO = serOsmand.Deserialize(reader) as OsmandExport.OsmChange;
            reader.Close();

            var user = "freeExec";
            bool positiveId = false;
            foreach(var a in args)
            {
                var b = a.ToLower();
                if (b.StartsWith("-u="))
                {
                    user = b.Substring(3);
                    break;
                } else if (b.Equals("--positiveid")) positiveId = true;
            }

            var notesJ = new JosmImport.OsmNotes();
            foreach(var note in notesO.notes)
            {
                var nnote = new JosmImport.Note();
                nnote.lon = note.lon;
                nnote.lat = note.lat;
                nnote.id = (positiveId) ? Math.Abs(note.id) : note.id;
                nnote.created_at = dateStr;

                nnote.comment = new JosmImport.Comment();
                nnote.comment.timestamp = dateStr;
                nnote.comment.user = user;
                nnote.comment.text = note.comment.text;

                notesJ.Add(nnote);
            }

            XmlSerializer serJosm = new XmlSerializer(typeof(JosmImport.OsmNotes));
            string exportName = (args.Length > 1 && !args[1].StartsWith("-")) ? args[1] : Path.ChangeExtension(filename, "osn");
            StreamWriter writer = new StreamWriter(exportName);
            serJosm.Serialize(writer, notesJ);
            reader.Close(); 
        }
    }
}
