using Play.Inventory.Service.Dtos;



namespace Play.Inventory.Service.Clients
{
    public class CatalogClient
    {
        private readonly HttpClient _httpClient;

        public CatalogClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemAsync()
        {
            var items = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("/items");
            if (items is null)
            {
                throw new Exception("Something went wrong calling the catalog API");
            } 
            return items;
        }
    }
}