namespace InfluxDb.Kapacitor;

public class KapacitorAlertDto
{
    public string id { get; set; }
    public string message { get; set; }
    public string level { get; set; }
    public KapactiorAlertData data { get; set; }
}

public class KapactiorAlertData
{
    public ICollection<KapacitorAlertSerie> series { get; set; }
}

public class KapacitorAlertSerie
{
    public KapacitorAlertTag tags { get; set; }
}

public class KapacitorAlertTag
{
    public string nome_provedor { get; set; }
    public string nome_recurso { get; set; }
}