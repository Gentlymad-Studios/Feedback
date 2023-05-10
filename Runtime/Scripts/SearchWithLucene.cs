using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using Directory = Lucene.Net.Store.Directory;

public class SearchWithLucene {


    private LuceneVersion version = LuceneVersion.LUCENE_48;

    private bool createIndexDocs = false;
    private IndexWriter writer;

    private StandardAnalyzer analyzer;
    private DirectoryReader directoryReader;
    private IndexSearcher indexSearcher;

    private string[] fieldsIncluded = { "Name", "Notes" };
    private  Directory indexDirectory;


    private static SearchWithLucene instance;
    public static SearchWithLucene Instance {
        get {
            if (instance == null)
                instance = new SearchWithLucene();
            return instance;
        }
    }

    private void SetupLucene() {
        indexDirectory = new RAMDirectory();
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

    /// <summary>
    /// Create a new Index at RAM Direcotry and Store all fetched tickets. 
    /// </summary>
    /// <param name="tickets"></param>
    public void CreateIndex(IEnumerable<TicketModels.AsanaTaskModel> tickets) {
        SetupLucene();
        string field = "0";
        if (createIndexDocs) {
            foreach (TicketModels.AsanaTaskModel ticket in tickets) {
                var document = new Document();
                try {
                    document.Add(new TextField("Gid", ticket.gid, Field.Store.YES));
                    document.Add(new TextField("Name", ticket.name, Field.Store.YES));
                    document.Add(new TextField("Notes", ticket.notes.ToString(), Field.Store.YES));

                    //Add field with discord likes if it exist
                    TicketModels.CustomField cf = ticket.custom_fields.Find(field => field.name == "Discord Likes");
                   
                    if (cf != null) {
                        field = cf.display_value.ToString();
                    }

                    document.Add(new TextField("Upvotes", field, Field.Store.YES));

                    Profiler.BeginSample("lucene writer sample");
                    writer.AddDocument(document);
                    Profiler.EndSample();
                } catch (Exception e) {
                    Debug.Log(e);
                }
            }

            Profiler.BeginSample("lucene commit sample");
            writer.Commit();
            Debug.Log("write to index");
            Profiler.EndSample();
        }
    }

    //public void UpdateIndex() { }

    /// <summary>
    /// Search the search term in the lucene index. 
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <returns></returns>
    public IEnumerable<TicketModels.AsanaTaskModel> SearchTerm(string searchTerm) {
        var results = new List<TicketModels.AsanaTaskModel>();
        try {
            directoryReader = DirectoryReader.Open(indexDirectory);
            indexSearcher = new IndexSearcher(directoryReader);
            QueryParser queryParser = new MultiFieldQueryParser(version, fieldsIncluded, analyzer);
            queryParser.AllowLeadingWildcard = true;
            Query searchQuery = queryParser.Parse(QueryParserBase.Escape(searchTerm));
           

            Debug.Log(searchQuery.ToString());
            ScoreDoc[] hits = indexSearcher.Search(searchQuery, null, 10).ScoreDocs;

          
            foreach (ScoreDoc hit in hits) {
                Document document = indexSearcher.Doc(hit.Doc);

                List<TicketModels.CustomField> customFields = new List<TicketModels.CustomField>();
                TicketModels.CustomField field = new TicketModels.CustomField();
                field.display_value = document.Get("Upvotes");
                customFields.Add(field);

                results.Add(new TicketModels.AsanaTaskModel() {
                    gid = document.Get("Gid"),
                    name = document.Get("Name"),
                    notes = document.Get("Notes"),
                    custom_fields = customFields,
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
