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
using UnityEngine;
using UnityEngine.Profiling;
using Directory = Lucene.Net.Store.Directory;

public class LuceneTest : MonoBehaviour {

    private LuceneVersion version = LuceneVersion.LUCENE_48;
    private string indexPath;

    private bool createIndexDocs = false;

    private StandardAnalyzer analyzer;
    private IndexWriter writer;
    private DirectoryReader directoryReader;
    private IndexSearcher indexSearcher;

    private string[] fieldsIncluded = { "Name", "Notes" };
    private Directory indexDirectory;

    private void OnEnable() {
        SetupLucene();
    }

    //Call in ShowWindow or onEnable
    private void SetupLucene() {
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
    }
    public void Dispose() {
        writer?.Dispose();
        directoryReader?.Dispose();
        indexDirectory?.Dispose();
        Debug.Log("Dispose the lucene stuff");
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
                } catch (Exception e) {
                    Debug.Log(e);
                }
            }

            Profiler.BeginSample("lucene commit sample");
            writer.Commit();
            Profiler.EndSample();
        }
    }

    /// <summary>
    /// Search the search term in the lucene index. 
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <returns></returns>
    public IEnumerable<TicketModels.AsanaTicketModel> SearchTerm(string searchTerm) {
        var results = new List<TicketModels.AsanaTicketModel>();
        try {
            directoryReader = DirectoryReader.Open(indexDirectory);
            indexSearcher = new IndexSearcher(directoryReader);

            QueryParser queryParser = new MultiFieldQueryParser(version, fieldsIncluded, analyzer);
            queryParser.AllowLeadingWildcard = true;
            Query searchQuery = queryParser.Parse(QueryParser.Escape(searchTerm));

            Debug.Log(searchQuery.ToString());
            ScoreDoc[] hits = indexSearcher.Search(searchQuery, null, 10).ScoreDocs;

            foreach (ScoreDoc hit in hits) {
                Document document = indexSearcher.Doc(hit.Doc);
                results.Add(new TicketModels.AsanaTicketModel() {
                    name = document.Get("Name"),
                    notes = document.Get("Notes"),
                });
            }

        } catch (Exception e) {
            Debug.Log(e);
        }
        return results;
    }

    /// <summary>
    /// A way to escape special characters from a given string
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <returns></returns>
    private string CustomEscape(string searchTerm) {
        char[] escapeCharSet = { '+', '-', '&', '|', '!', '(', ')', '{', '}', '[', ']', '^', '"', '~', '?', ':', '*' };
        char[] chars = searchTerm.ToCharArray();
        string escapedText = "";
        for (int i = 0; i < chars.Length; i++) {
            var esc = chars[i].ToString();
            if (escapeCharSet.Contains(chars[i])) {
                esc = "\\" + chars[i];
            }
            escapedText += esc;
        }
        return escapedText;
    }

}
