
using Codice.Client.Common;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;
using Directory = System.IO.Directory;

public class LuceneTest : MonoBehaviour {

    private LuceneVersion version = LuceneVersion.LUCENE_48;
    private string indexPath;

    private StandardAnalyzer analyzer;
    private IndexWriter writer;
    private Lucene.Net.Store.Directory indexDirectory;

    private void Start() {
        indexPath = Path.Combine(Application.persistentDataPath, "index");
        indexDirectory = FSDirectory.Open(indexPath);

        // Create an analyzer to process the text
        analyzer = new StandardAnalyzer(version);
 
        // Create an index writer
        var config = new IndexWriterConfig(version, analyzer);
        writer = new IndexWriter(indexDirectory, config);

        Debug.Log("Add lucene index file at directory: " + writer.Directory);
    }

    public void AddTicketsToIndex(IEnumerable<TicketModels.AsanaTicketModel> tickets) {

        var reader = DirectoryReader.Open(indexDirectory);
        for (int i = 0; i < reader.NumDocs; i++) {
            Document d = reader.Document(i);
            var fieldValuePairs = d.GetFields("Name");
            string log = "";
            foreach (var field in fieldValuePairs) {
                log += field;
            }
            Debug.Log(log);
        }

        if (Directory.GetFiles(Application.persistentDataPath + "/index").Length != 1) { // 1: because of writer.LOCK file
            return;
        }

        foreach (TicketModels.AsanaTicketModel ticket in tickets) {
            var document = new Document();
            try {
                document.Add(new StringField("Gid", ticket.gid.ToString(), Field.Store.YES));
                document.Add(new StringField("Name", ticket.name, Field.Store.YES));
                document.Add(new StringField("Notes", ticket.notes.ToString(), Field.Store.YES));
                document.Add(new StringField("Created at", ticket.created_at.ToString(), Field.Store.YES));
                writer.AddDocument(document);
                Debug.Log("Add ticket with name: " + ticket.name.ToString() +"\n" + "actual number of docs in index: " + writer.NumDocs);
            } catch (Exception e) {
                Debug.Log(e);
            }
        }
        writer.Commit();
    }

    public IEnumerable<TicketModels.AsanaTicketModel> SearchTerm(string searchTerm) {


        DirectoryReader directoryReader = DirectoryReader.Open(indexDirectory);
        IndexSearcher indexSearcher = new IndexSearcher(directoryReader);

        string[] fieldsIncluded = { "Gid", "Name", "Notes", "Created at" };
        //string[] fieldsIncluded = { "Name", "Notes" };
        QueryParser queryParser = new MultiFieldQueryParser(version, fieldsIncluded, analyzer);
        Query searchQuery = queryParser.Parse(searchTerm);
        Debug.Log(searchQuery.ToString());
        ScoreDoc[] hits = indexSearcher.Search(searchQuery, 10).ScoreDocs;

        var results = new List<TicketModels.AsanaTicketModel>();

        foreach (ScoreDoc hit in hits) {
            Document document = indexSearcher.Doc(hit.Doc);
            results.Add(new TicketModels.AsanaTicketModel() {
                gid = document.Get("Gid"),
                name = document.Get("Name"),
                notes = document.Get("Notes"),
                created_at = document.Get("Created at")
            });
        }
        return results;
    }

}
