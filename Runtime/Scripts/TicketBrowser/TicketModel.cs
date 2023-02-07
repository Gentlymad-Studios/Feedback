using System.Collections.Generic;

// TODO: I guess this class should be asana specific? If you plan to have a common data for other implementations this might still make sense but then we would need a converter class that can transform the data to the required fields and types
// for me it would be okay to focus on asana only for now, but I guess it would be cleaner if this would become the AsanaTicketModel then to remind us that this is not platform agnostic
public class TicketModel {
    public string gid { get; set; }
    public string created_at { get; set; }
    public string name { get; set; }
    public string notes { get; set; }

}

// TODO: with this class you can directly deserialize the data without having to format it or do any string formatting (replace is super expensive)
// As a side note (maybe this was not possible before due to the stream reader appending data), you can alwas use: https://json2csharp.com/ to convert json to the needed data types
public class TicketModels {
    public List<TicketModel> data;
}