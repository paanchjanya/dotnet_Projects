using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using frontend.Models;

namespace frontend.Services
{
    public class CartService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string CartKey = "shopping_cart";

        public event Action? OnCartChanged;

        public CartService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<List<CartItem>> GetCartItemsAsync()
        {
            try
            {
                var cartJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", CartKey);
                if (string.IsNullOrWhiteSpace(cartJson))
                {
                    return new List<CartItem>();
                }
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        public async Task AddToCartAsync(Book book, int quantity = 1)
        {
            var cart = await GetCartItemsAsync();
            var item = cart.FirstOrDefault(i => i.Book.Id == book.Id);
            if (item == null)
            {
                cart.Add(new CartItem { Book = book, Quantity = quantity });
            }
            else
            {
                item.Quantity += quantity;
            }
            await SaveCartAsync(cart);
            OnCartChanged?.Invoke();
        }

        public async Task UpdateQuantityAsync(int bookId, int quantity)
        {
            var cart = await GetCartItemsAsync();
            var item = cart.FirstOrDefault(i => i.Book.Id == bookId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                await SaveCartAsync(cart);
                OnCartChanged?.Invoke();
            }
        }

        public async Task RemoveItemAsync(int bookId)
        {
            var cart = await GetCartItemsAsync();
            var item = cart.FirstOrDefault(i => i.Book.Id == bookId);
            if (item != null)
            {
                cart.Remove(item);
                await SaveCartAsync(cart);
                OnCartChanged?.Invoke();
            }
        }

        public async Task ClearCartAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", CartKey);
            OnCartChanged?.Invoke();
        }

        public async Task<int> GetTotalCountAsync()
        {
            var cart = await GetCartItemsAsync();
            return cart.Sum(i => i.Quantity);
        }

        public async Task<decimal> GetTotalAmountAsync()
        {
            var cart = await GetCartItemsAsync();
            return cart.Sum(i => i.Book.Price * i.Quantity);
        }

        private async Task SaveCartAsync(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CartKey, cartJson);
        }
    }
}
