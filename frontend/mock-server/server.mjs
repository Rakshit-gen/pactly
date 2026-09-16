// Small stand-in for the real GraphQL API, used only for local frontend development
// before the backend is deployed. Not part of the production build.
import { createServer } from 'node:http';

const PORT = process.env.MOCK_PORT ?? 5236;

const products = [
  {
    id: 'p-starter',
    slug: 'starter',
    name: 'Starter',
    description: 'For small teams sending their first few agreements.',
    type: 'PLAN',
    monthlyPriceCents: 2900,
    annualPriceCents: 29000,
    seatLimit: 5,
    features: ['Up to 5 seats', '20 agreements per month', 'Standard audit trail', 'Email support'],
  },
  {
    id: 'p-growth',
    slug: 'growth',
    name: 'Growth',
    description: 'For teams that live in contracts every day.',
    type: 'PLAN',
    monthlyPriceCents: 5900,
    annualPriceCents: 59000,
    seatLimit: 20,
    features: ['Up to 20 seats', 'Unlimited agreements', 'Detailed audit trail with export', 'Priority support'],
  },
  {
    id: 'p-scale',
    slug: 'scale',
    name: 'Scale',
    description: 'For organizations running agreements as infrastructure.',
    type: 'PLAN',
    monthlyPriceCents: 14900,
    annualPriceCents: 149000,
    seatLimit: null,
    features: ['Unlimited seats', 'Unlimited agreements', 'Custom retention policies', 'Dedicated support'],
  },
  {
    id: 'p-storage',
    slug: 'extra-storage',
    name: 'Extra document storage',
    description: 'An additional 50 GB of signed document storage.',
    type: 'ADD_ON',
    monthlyPriceCents: 900,
    annualPriceCents: 9000,
    seatLimit: null,
    features: ['50 GB additional storage'],
  },
  {
    id: 'p-support',
    slug: 'priority-support',
    name: 'Priority support',
    description: 'Skip the queue with a two hour response window.',
    type: 'ADD_ON',
    monthlyPriceCents: 1500,
    annualPriceCents: 15000,
    seatLimit: null,
    features: ['Two hour response window', 'Direct Slack channel'],
  },
];

const user = {
  id: 'u-1',
  email: 'jane@example.com',
  displayName: 'Jane Alvarez',
  currentPlanId: null,
  currentSeats: 0,
  currentPeriodEnd: null,
};

let cart = { id: 'cart-1', lines: [] };
const orders = [];
const agreements = [];

function withProduct(line) {
  return { ...line, product: products.find((p) => p.id === line.productId) ?? null };
}

function buildContentSnapshot(order) {
  const lines = order.lines
    .map((l) => `- ${l.productName} x${l.quantity} at $${(l.unitPriceCents / 100).toFixed(2)} per month`)
    .join('\n');
  return `Agreement for ${user.displayName} (${user.email})\n\n${lines}\n\nTotal due today: $${(order.totalCents / 100).toFixed(2)}`;
}

const handlers = {
  Products: () => ({ products }),
  Me: () => ({ me: user }),
  Cart: () => ({ cart: { ...cart, lines: cart.lines.map(withProduct) } }),
  MyOrders: () => ({ myOrders: orders }),
  MyAgreements: () => ({ myAgreements: agreements }),
  Agreement: (variables) => ({ agreement: agreements.find((a) => a.id === variables.id) ?? null }),

  Register: (variables) => {
    user.displayName = variables.displayName;
    user.email = variables.email.toLowerCase();
    return { register: { token: 'mock-token', user } };
  },
  Login: () => ({ login: { token: 'mock-token', user } }),

  AddToCart: (variables) => {
    const existing = cart.lines.find((l) => l.productId === variables.productId);
    if (existing) {
      existing.quantity = variables.quantity;
    } else {
      cart.lines.push({ productId: variables.productId, quantity: variables.quantity });
    }
    return { addToCart: { ...cart, lines: cart.lines.map(withProduct) } };
  },
  RemoveFromCart: (variables) => {
    cart.lines = cart.lines.filter((l) => l.productId !== variables.productId);
    return { removeFromCart: { ...cart, lines: cart.lines.map(withProduct) } };
  },

  Checkout: () => {
    const lines = cart.lines.map((line) => {
      const product = products.find((p) => p.id === line.productId);
      return {
        productId: product.id,
        productName: product.name,
        unitPriceCents: product.monthlyPriceCents,
        quantity: line.quantity,
      };
    });
    const subtotalCents = lines.reduce((sum, l) => sum + l.unitPriceCents * l.quantity, 0);
    const order = {
      id: `order-${orders.length + 1}`,
      status: 'PENDING',
      subtotalCents,
      prorationCreditCents: 0,
      totalCents: subtotalCents,
      createdAt: new Date().toISOString(),
      lines,
    };
    orders.unshift(order);

    const agreement = {
      id: `agreement-${agreements.length + 1}`,
      orderId: order.id,
      status: 'AWAITING_SIGNATURE',
      contentSnapshot: buildContentSnapshot(order),
      signatureDataUrl: null,
      signedAt: null,
      executedAt: null,
      auditTrail: [
        { timestamp: new Date().toISOString(), actor: 'system', action: 'Created', metadata: `Agreement drafted for order ${order.id}.` },
        { timestamp: new Date().toISOString(), actor: 'system', action: 'SentForSignature', metadata: 'Agreement moved to awaiting signature.' },
      ],
    };
    agreements.unshift(agreement);
    cart = { id: cart.id, lines: [] };

    return { checkout: { order, agreement } };
  },

  SignAgreement: (variables) => {
    const agreement = agreements.find((a) => a.id === variables.agreementId);
    const now = new Date().toISOString();
    agreement.status = 'EXECUTED';
    agreement.signatureDataUrl = variables.signatureDataUrl;
    agreement.signedAt = now;
    agreement.executedAt = now;
    agreement.auditTrail.push(
      { timestamp: now, actor: user.email, action: 'Signed', metadata: 'Signature captured from checkout flow.' },
      { timestamp: now, actor: 'Pactly', action: 'Executed', metadata: 'Countersigned and executed on behalf of Pactly.' },
    );
    const order = orders.find((o) => o.id === agreement.orderId);
    if (order) {
      order.status = 'COMPLETED';
      const planLine = order.lines[0];
      const product = products.find((p) => p.id === planLine.productId);
      if (product?.type === 'PLAN') {
        user.currentPlanId = product.id;
        user.currentSeats = planLine.quantity;
        user.currentPeriodEnd = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString();
      }
    }
    return { signAgreement: agreement };
  },
};

const server = createServer((req, res) => {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Headers', 'content-type, authorization');
  res.setHeader('Access-Control-Allow-Methods', 'POST, OPTIONS');

  if (req.method === 'OPTIONS') {
    res.writeHead(204);
    res.end();
    return;
  }

  if (req.method !== 'POST') {
    res.writeHead(404);
    res.end();
    return;
  }

  let body = '';
  req.on('data', (chunk) => {
    body += chunk;
  });
  req.on('end', () => {
    try {
      const { operationName, variables } = JSON.parse(body);
      const handler = handlers[operationName];
      if (!handler) {
        res.writeHead(400, { 'content-type': 'application/json' });
        res.end(JSON.stringify({ errors: [{ message: `No mock handler for ${operationName}` }] }));
        return;
      }
      const data = handler(variables ?? {});
      res.writeHead(200, { 'content-type': 'application/json' });
      res.end(JSON.stringify({ data }));
    } catch (err) {
      res.writeHead(500, { 'content-type': 'application/json' });
      res.end(JSON.stringify({ errors: [{ message: String(err) }] }));
    }
  });
});

server.listen(PORT, () => {
  console.log(`Mock Pactly GraphQL server listening on http://localhost:${PORT}/graphql`);
});
