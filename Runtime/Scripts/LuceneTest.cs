using UnityEngine;
using System.IO;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;

using Directory = Lucene.Net.Store.Directory;
using LuceneVersion = Lucene.Net.Util.Version;
public class LuceneTest : MonoBehaviour {

    private LuceneVersion version = LuceneVersion.LUCENE_30;
    private string indexPath;

    private void Start() {
        indexPath = Path.Combine(Application.persistentDataPath, "index");
        using Directory indexDirectory = FSDirectory.Open(indexPath);

        // Create an analyzer to process the text
        var analyzer = new StandardAnalyzer(version);

        // Create an index writer
        using IndexWriter writer = new IndexWriter(indexDirectory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

        Debug.Log("Add lucene index file at directory: " + writer.Directory);
    }
}
