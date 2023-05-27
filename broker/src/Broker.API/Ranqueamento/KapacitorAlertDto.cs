// ReSharper disable InconsistentNaming

// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable CollectionNeverUpdated.Global

namespace Broker.API.Ranqueamento;

public class KapacitorAlertDto
{
    public string id { get; set; }
    public string level { get; set; }
    public KapacitorAlertData data { get; set; }
}

public class KapacitorAlertData
{
    public ICollection<KapacitorAlertSerie> series { get; set; }
}

public class KapacitorAlertSerie
{
    public KapacitorAlertTag tags { get; set; }
    public List<string> columns { get; set; }
    public List<object> values { get; set; }
}

public class KapacitorAlertTag
{
    public string nome_provedor { get; set; }
    public string nome_recurso { get; set; }
}