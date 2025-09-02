<template>
  <div class="shopping-cart-navbar">
    <button class="cart-button" @click="showCart = !showCart">
      🛒 <span v-if="cartItems.orderItems && cartItems.orderItems.length > 0" class="cart-badge">{{ cartItems.orderItems.length }}</span>
    </button>
    <div v-if="showCart" class="cart-content">
      <div class="cart-header">
        <span class="cart-title">Shopping Cart</span>
        <button class="close-btn" @click="showCart = false">×</button>
      </div>
      <div v-if="!cartItems.orderItems || cartItems.orderItems.length === 0" class="cart-empty">Cart is empty</div>
      <div v-else>
        <div v-for="item in cartItems.orderItems || []" :key="item.id" class="cart-item">
          <span class="cart-item-title">{{ item.tourName }}</span>
          <span class="cart-item-price">{{ item.price }} €</span>
          <button class="remove-btn" @click="removeFromCart(item.id)">🗑️</button>
        </div>
        <div class="cart-total">
          <span>Total:</span>
          <span class="cart-total-price">{{ totalPrice }} €</span>
        </div>
      </div>
      <div class="cart-actions">
        <button @click="showCart = false">Continue Shopping</button>
        <button :disabled="!cartItems.orderItems || cartItems.orderItems.length === 0" @click="checkout">Checkout</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios';
const showCart = ref(false)
const cartItems = ref([])
const totalPrice = ref(0)

const API_URL = 'http://localhost:8082/api/tours'

async function fetchCart() {
  const jwt = localStorage.getItem('token');
  try {
    const response = await axios.get(`${API_URL}/gettoursfromcart`, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    console.log('Cart items fetched:', response.data.value);

    // postavljamo ceo objekat
    cartItems.value = response.data.value || { orderItems: [] };

    // racunamo ukupnu cenu preko orderItems
    if (cartItems.value.orderItems && Array.isArray(cartItems.value.orderItems)) {
      totalPrice.value = cartItems.value.orderItems.reduce((sum, item) => sum + item.price, 0);
    } else {
      totalPrice.value = 0;
    }
  } catch (err) {
    console.error('Error fetching cart:', err);
    cartItems.value = { orderItems: [] };
    totalPrice.value = 0;
  }
}

async function removeFromCart(orderItemId) {
  const jwt = localStorage.getItem('token');
  try {
    console.log(`Attempting to delete item with ID: ${orderItemId}`);
    const response = await axios.delete(`${API_URL}/deletetourtocart/${orderItemId}`, {
      headers: { Authorization: `Bearer ${jwt}` }
    });
    console.log('Item deleted successfully:', response.data);
    
    await fetchCart();
    console.log('Cart refreshed after deletion');
  } catch (err) {
    console.error('Error deleting item from cart:', err);
    alert('Item deletion failed!');
  }
}


async function checkout() {
  const jwt = localStorage.getItem('token');
  try {
    // Prepare data in the format expected by backend (OrderItemFromCartDTO)
    const orderItemsForPurchase = cartItems.value.orderItems.map(item => ({
      id: item.id,
      shoppingCartId: cartItems.value.id,
      tourId: item.tourId,
      price: item.price
    }));

    console.log('Sending checkout data:', orderItemsForPurchase);

    const response = await axios.post(`${API_URL}/buytours`, orderItemsForPurchase, {
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${jwt}`
      }
    });

    console.log('Purchase successful:', response.data);
    alert('Purchase successful! You now own these tours and can start them from the Tours page.');
    
    // Trigger events to refresh other components
    window.dispatchEvent(new CustomEvent('cart-updated'));
    window.dispatchEvent(new CustomEvent('tours-purchased'));
    
    await fetchCart(); // Refresh cart after successful purchase
    showCart.value = false;
  } catch (err) {
    console.error('Purchase failed:', err);
    console.error('Error response:', err.response?.data);
    alert('Purchase failed! Please try again.');
  }
}

onMounted(() => {
  fetchCart();
  
  // Listen for cart updates
  window.addEventListener('cart-updated', fetchCart);
  
  // Legacy event listener for opening cart
  window.addEventListener('open-cart', () => {
    fetchCart();
    showCart.value = true;
  });
});
</script>

<style scoped>
  .shopping-cart-navbar {
    position: relative;
    display: inline-block;
  }
  .cart-button {
    background: transparent;
    border: none;
    color: #667eea;
    border-radius: 50px;
    padding: 0.5rem 1rem;
    font-size: 1.2rem;
    cursor: pointer;
    position: relative;
    transition: color 0.3s ease;
  }
  .cart-button:hover {
    color: #4c63d2;
  }
  .cart-badge {
    background: #e53e3e;
    color: #fff;
    border-radius: 50%;
    padding: 0.2rem 0.5rem;
    font-size: 0.8rem;
    position: absolute;
    top: -5px;
    right: -5px;
    min-width: 18px;
    text-align: center;
  }
  .cart-content {
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 4px 24px rgba(0,0,0,0.15);
    min-width: 320px;
    max-width: 400px;
    padding: 1rem;
    position: absolute;
    top: 100%;
    right: 0;
    margin-top: 0.5rem;
    z-index: 1000;
  }
  .cart-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }
  .cart-title {
    font-size: 1.2rem;
    font-weight: bold;
  }
  .close-btn {
    background: none;
    border: none;
    font-size: 1.5rem;
    cursor: pointer;
  }
  .cart-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.5rem 0;
    border-bottom: 1px solid #eee;
  }
  .cart-item-title {
    flex: 1;
    font-weight: 500;
  }
  .cart-item-price {
    margin-left: 1rem;
    color: #555;
  }
  .remove-btn {
    background: none;
    border: none;
    font-size: 1.2rem;
    color: #e53e3e;
    cursor: pointer;
    margin-left: 1rem;
  }
  .cart-total {
    display: flex;
    justify-content: space-between;
    align-items: center;
    background: #f5f5f5;
    padding: 0.7rem 1rem;
    border-radius: 8px;
    margin-top: 1rem;
    font-weight: bold;
  }
  .cart-total-price {
    color: #2b6cb0;
    font-size: 1.1rem;
  }
  .cart-empty {
    text-align: center;
    color: #888;
    margin: 2rem 0;
  }
  .cart-actions {
    display: flex;
    justify-content: space-between;
    margin-top: 1.5rem;
  }
  .cart-actions button {
    background: #667eea;
    color: #fff;
    border: none;
    border-radius: 6px;
    padding: 0.5rem 1.2rem;
    font-size: 1rem;
    cursor: pointer;
    transition: background 0.2s;
  }
  .cart-actions button:disabled {
    background: #ccc;
    cursor: not-allowed;
  }
</style>
