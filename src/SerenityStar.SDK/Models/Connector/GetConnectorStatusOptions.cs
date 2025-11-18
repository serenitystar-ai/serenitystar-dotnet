namespace SerenityStar.Models.Connector;

public class GetConnectorStatusOptions
{
    public Guid AgentInstanceId { get; set; }
    public Guid ConnectorId { get; set; }
}
