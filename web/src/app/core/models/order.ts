export interface OrderItem {
  orderId: string;
  productId: string;
  quantity: number;
  price: number;
}

export interface Address {
  firstName: string;
  lastName: string;
  emailAddress: string;
  addressLine: string;
  country: string;
  state: string;
  zipCode: string;
}

export interface Payment {
  cardName: string;
  cardNumber: string;
  expiration: string;
  cvv: string;
  paymentMethod: number;
}

export interface Order {
  id: string;
  customerId: string;
  name: string;
  shippingAddress: Address;
  billingAddress: Address;
  payment: Payment;
  status: string;
  items: OrderItem[];
}
