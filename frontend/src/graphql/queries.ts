import { gql } from '@apollo/client';

export const PRODUCTS_QUERY = gql`
  query Products {
    products {
      id
      slug
      name
      description
      type
      monthlyPriceCents
      annualPriceCents
      features
      seatLimit
    }
  }
`;

export const ME_QUERY = gql`
  query Me {
    me {
      id
      email
      displayName
      currentPlanId
      currentSeats
      currentPeriodEnd
    }
  }
`;

export const CART_QUERY = gql`
  query Cart {
    cart {
      id
      lines {
        productId
        quantity
        product {
          id
          name
          slug
          type
          monthlyPriceCents
        }
      }
    }
  }
`;

export const MY_ORDERS_QUERY = gql`
  query MyOrders {
    myOrders {
      id
      status
      subtotalCents
      prorationCreditCents
      totalCents
      createdAt
      lines {
        productId
        productName
        unitPriceCents
        quantity
      }
    }
  }
`;

export const MY_AGREEMENTS_QUERY = gql`
  query MyAgreements {
    myAgreements {
      id
      orderId
      status
      contentSnapshot
      signedAt
      executedAt
    }
  }
`;

export const AGREEMENT_QUERY = gql`
  query Agreement($id: String!) {
    agreement(id: $id) {
      id
      orderId
      status
      contentSnapshot
      signatureDataUrl
      signedAt
      executedAt
      auditTrail {
        timestamp
        actor
        action
        metadata
      }
    }
  }
`;
