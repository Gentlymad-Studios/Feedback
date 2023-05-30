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
using System.Threading.Tasks;
using UnityEngine;
using Directory = Lucene.Net.Store.Directory;

public class SearchWithLucene {

    private LuceneVersion version = LuceneVersion.LUCENE_48;

    private IndexWriter writer;
    private StandardAnalyzer analyzer;
    private DirectoryReader directoryReader;
    private IndexSearcher indexSearcher;
    private Directory indexDirectory;

    private bool createIndexDocs = false;
    private string[] fieldsIncluded = { "Name", "Notes" };


    private static SearchWithLucene instance;
    public static SearchWithLucene Instance {
        get {
            if (instance == null)
                instance = new SearchWithLucene();
            return instance;
        }
    }

    public void Dispose() {
        indexDirectory?.Dispose();
        analyzer?.Dispose();
    }


    //TODO: generic task type
    /// <summary>
    /// Create a new Index at RAM Direcotry and Store all fetched tickets. 
    /// </summary>
    /// <param name="tasks"></param>
    public void CreateIndex(IEnumerable<TaskModels.AsanaTaskModel> tasks) {
        SetupLucene();
        foreach (TaskModels.AsanaTaskModel task in tasks) {
            var document = new Document();
            try {
                document.Add(new TextField("Gid", task.gid, Field.Store.YES));
                document.Add(new TextField("Name", task.name, Field.Store.YES));
                document.Add(new TextField("Notes", task.notes.ToString(), Field.Store.YES));

                TaskModels.CustomField cf = task.custom_fields.Find(field => field.name == "Upvotes");
                string field = cf.display_value.ToString();
                document.Add(new TextField("Upvotes", field, Field.Store.YES));

                writer.AddDocument(document);
            } catch (Exception e) {
                Debug.Log(e);
            }

            //Apply all changes so index
            writer.Commit();
        }
        //Dispose writer, no changes possible till new initalisation of index writer
        writer.Dispose();
    }


    /// <summary>
    /// Search the search term in the lucene index. 
    /// </summary>
    /// <param name="searchTerm"></param>
    /// <returns></returns>
    public IEnumerable<TaskModels.AsanaTaskModel> SearchTerm(string searchTerm) {
        var results = new List<TaskModels.AsanaTaskModel>();
        try {
            directoryReader = DirectoryReader.Open(indexDirectory);
            indexSearcher = new IndexSearcher(directoryReader);
            QueryParser queryParser = new MultiFieldQueryParser(version, fieldsIncluded, analyzer);
            queryParser.AllowLeadingWildcard = true;
            Query searchQuery = queryParser.Parse(QueryParserBase.Escape(searchTerm) + "*");

            ScoreDoc[] hits = indexSearcher.Search(searchQuery, null, 10).ScoreDocs;

            foreach (ScoreDoc hit in hits) {
                Document document = indexSearcher.Doc(hit.Doc);

                List<TaskModels.CustomField> customFields = new List<TaskModels.CustomField>();
                TaskModels.CustomField field = new TaskModels.CustomField();
                field.display_value = document.Get("Upvotes");
                customFields.Add(field);

                results.Add(new TaskModels.AsanaTaskModel() {
                    gid = document.Get("Gid"),
                    name = document.Get("Name"),
                    notes = document.Get("Notes"),
                    custom_fields = customFields,
                });
            }

        } catch (Exception e) {
            Debug.LogException(e);
        }
        return results;
    }

    /// <summary>
    /// Create Ram directory, config and analyzer
    /// https://lucenenet.apache.org/docs/4.8.0-beta00016/api/core/Lucene.Net.Store.RAMDirectory.html
    /// The RAMDirectory will probably not work with huge indexes(up to several 100 hundred megabytes)
    /// </summary>
    private void SetupLucene() {
        try {
            indexDirectory = new RAMDirectory();
            analyzer = new StandardAnalyzer(version);
            var config = new IndexWriterConfig(version, analyzer);
            config.OpenMode = OpenMode.CREATE_OR_APPEND;
            writer = new IndexWriter(indexDirectory, config);
            createIndexDocs = true;
            Debug.Log("Add lucene index file at directory: " + writer.Directory);
        } catch (Exception e) {
            Debug.LogException(e);
        }
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
