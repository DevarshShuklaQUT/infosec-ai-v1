using Microsoft.AspNetCore.Mvc;

namespace InfoSecApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly ILogger<DataController> _logger;

    public DataController(ILogger<DataController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<DataItem>> GetData()
    {
        _logger.LogInformation("GetData endpoint called");
        
        var data = new List<DataItem>
        {
            new DataItem { Id = 1, Name = "Item 1", Description = "First item" },
            new DataItem { Id = 2, Name = "Item 2", Description = "Second item" },
            new DataItem { Id = 3, Name = "Item 3", Description = "Third item" }
        };

        return Ok(data);
    }

    [HttpGet("{id}")]
    public ActionResult<DataItem> GetDataItem(int id)
    {
        _logger.LogInformation("GetDataItem endpoint called with id: {Id}", id);
        
        var item = new DataItem { Id = id, Name = $"Item {id}", Description = $"Description for item {id}" };
        return Ok(item);
    }

    [HttpPost]
    public ActionResult<DataItem> CreateDataItem(CreateDataItemRequest request)
    {
        _logger.LogInformation("CreateDataItem endpoint called");
        
        var item = new DataItem 
        { 
            Id = Random.Shared.Next(100, 999), 
            Name = request.Name, 
            Description = request.Description 
        };
        
        return CreatedAtAction(nameof(GetDataItem), new { id = item.Id }, item);
    }
}

public class DataItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class CreateDataItemRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
