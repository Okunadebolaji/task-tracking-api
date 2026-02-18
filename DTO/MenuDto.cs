public class MenuDto
{
    public int MenuId { get; set; }
    public string Name { get; set; } = null!;
    public string? Route { get; set; }
    public string? Icon { get; set; }
    public int? ParentMenuId { get; set; }
    public List<MenuDto> Children { get; set; } = new();
}
