export interface ShoppingCartItem {
  quantity: number;
  price: number;
  productId: string;
  productName: string;
  description?: string;
}

export interface ShoppingCart {
  username: string;
  items: ShoppingCartItem[];
  totalPrice: number;
}

export interface CheckoutPayload {
  username: string;
  customerId: string;
  firstName: string;
  lastName: string;
  emailAddress: string;
  addressLine: string;
  country: string;
  state: string;
  zipCode: string;
  cardName: string;
  cardNumber: string;
  expiration: string;
  cvv: string;
  paymentMethod: number;
}
