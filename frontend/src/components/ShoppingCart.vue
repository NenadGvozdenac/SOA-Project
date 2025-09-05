<template>
  <div class="shopping-cart">
    <button class="cart-button" @click="showCart = !showCart">
      🛒 <span v-if="cartItems.length > 0" class="cart-badge">{{ cartItems.length }}</span>
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
        <button :disabled="cartItems.orderItems.length === 0" @click="checkout">Checkout</button>
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
    totalPrice.value = cartItems.value.orderItems.reduce((sum, item) => sum + item.price, 0);
  } catch (err) {
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
    await axios.post(`${API_URL}/buytours`, cartItems.value.orderItems || [], {
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${jwt}`
      }
    });
    await fetchCart();
    showCart.value = false;
  } catch (err) {
    alert('Purchase failed!');
  }
}

onMounted(() => {
  fetchCart();
  window.addEventListener('open-cart', () => {
    fetchCart();
    showCart.value = true;
  });
});
</script>

<style scoped>
  .shopping-cart {
    position: fixed;
    top: 5rem;
    right: 1rem;
    z-index: 1000;
  }
  .cart-button {
    background: #fff;
    border: 1px solid #ddd;
    border-radius: 50px;
    padding: 0.5rem 1rem;
    font-size: 1.5rem;
    cursor: pointer;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    position: relative;
  }
  .cart-badge {
    background: #e53e3e;
    color: #fff;
    border-radius: 50%;
    padding: 0.2rem 0.6rem;
    font-size: 1rem;
    position: absolute;
    top: -8px;
    right: -8px;
  }
  .cart-content {
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 4px 24px rgba(0,0,0,0.08);
    min-width: 320px;
    max-width: 400px;
    padding: 1rem;
    position: absolute;
    top: 48px;
    right: 0;
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
