export interface AuthUser {
  id: string;
  email: string;
  displayName: string;
}

export type ProductType = 'PLAN' | 'ADD_ON';

export interface Product {
  id: string;
  slug: string;
  name: string;
  description: string;
  type: ProductType;
  monthlyPriceCents: number;
  annualPriceCents: number;
  features: string[];
  seatLimit: number | null;
}

export interface CartLine {
  productId: string;
  quantity: number;
  product: Product | null;
}

export interface Cart {
  id: string;
  lines: CartLine[];
}

export interface OrderLine {
  productId: string;
  productName: string;
  unitPriceCents: number;
  quantity: number;
}

export type OrderStatus = 'PENDING' | 'COMPLETED' | 'CANCELLED';

export interface Order {
  id: string;
  status: OrderStatus;
  subtotalCents: number;
  prorationCreditCents: number;
  totalCents: number;
  createdAt: string;
  lines: OrderLine[];
}

export type AgreementStatus = 'DRAFT' | 'AWAITING_SIGNATURE' | 'SIGNED' | 'EXECUTED';

export interface AuditEntry {
  timestamp: string;
  actor: string;
  action: string;
  metadata: string;
}

export interface Agreement {
  id: string;
  orderId: string;
  status: AgreementStatus;
  contentSnapshot: string;
  signatureDataUrl?: string | null;
  signedAt?: string | null;
  executedAt?: string | null;
  auditTrail?: AuditEntry[];
}
