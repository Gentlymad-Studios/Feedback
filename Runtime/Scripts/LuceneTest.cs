using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Profiling;
using Directory = Lucene.Net.Store.Directory;

public class LuceneTest : MonoBehaviour {

    private LuceneVersion version = LuceneVersion.LUCENE_48;
    private string indexPath;

    private bool createIndexDocs = false;
    private bool disposeLucene = false;

    private StandardAnalyzer analyzer;
    private IndexWriter writer;
    private DirectoryReader directoryReader;
    private IndexSearcher indexSearcher;

    private string[] fieldsIncluded = { "Name", "Notes" };
    private int count = 0;
    private Directory indexDirectory;

    private void Start() {

        indexPath = Path.Combine(Application.persistentDataPath, "Index");
        if (!System.IO.Directory.Exists(indexPath)) {
            System.IO.Directory.CreateDirectory(indexPath);
        }
        indexDirectory = FSDirectory.Open(indexPath);

        //indexDirectory = new RAMDirectory();
      
        // Create an analyzer to process the text
        analyzer = new StandardAnalyzer(version);
        var config = new IndexWriterConfig(version, analyzer);

        if (!DirectoryReader.IndexExists(indexDirectory)) {
            // Create an index writer if no index exists 
            writer = new IndexWriter(indexDirectory, config);
            createIndexDocs = true;
            Debug.Log("Add lucene index file at directory: " + writer.Directory);
        }
        Profiler.EndSample();
    }

    private void Update() {

        if (disposeLucene) {
            Dispose();
            disposeLucene = false;
        }

    }
    public void Dispose() {
        writer?.Dispose();
        directoryReader?.Dispose();
        indexDirectory?.Dispose();
        Debug.Log("Dispose the lucene stuff");
    }

    public void SetDisposeLuceneTools(bool disposeLuceneTools) {
        this.disposeLucene = disposeLuceneTools;
    }

    public void AddTicketsToIndex(IEnumerable<TicketModels.AsanaTicketModel> tickets) {

        if (createIndexDocs) {
            foreach (TicketModels.AsanaTicketModel ticket in tickets) {
                var document = new Document();
                try {
                    document.Add(new TextField("Name", ticket.name, Field.Store.YES));
                    document.Add(new TextField("Notes", ticket.notes.ToString(), Field.Store.YES));
                    Profiler.BeginSample("lucene writer sample");
                    writer.AddDocument(document);
                    Profiler.EndSample();

                    count++;
                } catch (Exception e) {
                    Debug.Log(e);
                }
            }

            Profiler.BeginSample("lucene commit sample");
            writer.Commit();
            Profiler.EndSample();

            Debug.Log(count);

        }
    }

    /// <summary>
    /// Search the search term in the lucene index. 
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <returns></returns>
    public IEnumerable<TicketModels.AsanaTicketModel> SearchTerm(string searchTerm) {
        string escapedSearchTerm = ModifySearchTerm(EscapeSpecialChars(searchTerm), new string[]{ "*" });

        directoryReader = DirectoryReader.Open(indexDirectory);
        indexSearcher = new IndexSearcher(directoryReader);

        QueryParser queryParser = new MultiFieldQueryParser(version, fieldsIncluded, analyzer);
        queryParser.AllowLeadingWildcard = true;
        Query searchQuery = queryParser.Parse(escapedSearchTerm.Trim());
      
        Debug.Log(searchQuery.ToString());
        ScoreDoc[] hits = indexSearcher.Search(searchQuery, null, 5000).ScoreDocs;

        Debug.Log(hits.Length);
        var results = new List<TicketModels.AsanaTicketModel>();

        foreach (ScoreDoc hit in hits) {
            Document document = indexSearcher.Doc(hit.Doc);
            results.Add(new TicketModels.AsanaTicketModel() {
                name = document.Get("Name"),
                notes = document.Get("Notes"),
            });
        }
        return results;
    }

    /// <summary>
    /// Modify a string with the prefixes in the string array. The prefixes will be added in the same order as in the array.
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <returns></returns>
    private string ModifySearchTerm(string searchTerm, string[] prefix) {
        foreach(string pre in prefix) {
            searchTerm = pre + searchTerm;
        }
        return searchTerm;
    }

    /// <summary>
    /// A way to escape special characters from a given string
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <returns></returns>
    private string EscapeSpecialChars(string searchTerm) {
        char[] escapingChars = { '+', '-', '&', '|', '!', '(', ')', '{', '}', '[', ']', '^', '"', '~', '?', ':', '*'};
        char[] chars = searchTerm.ToCharArray();
        string escapedText = "";
        for(int i = 0; i < chars.Length; i++) {
            var esc = chars[i].ToString();
            if (escapingChars.Contains(chars[i])) {
                esc = "\\" + chars[i];
            }
            escapedText += esc;
        }
        return escapedText;
    }

}
