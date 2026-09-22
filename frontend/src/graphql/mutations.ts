import { gql } from '@apollo/client';

export const REGISTER_MUTATION = gql`
  mutation Register($email: String!, $password: String!, $displayName: String!) {
    register(email: $email, password: $password, displayName: $displayName) {
      token
      user {
        id
        email
        displayName
      }
    }
  }
`;

export const LOGIN_MUTATION = gql`
  mutation Login($email: String!, $password: String!) {
    login(email: $email, password: $password) {
      token
      user {
        id
        email
        displayName
      }
    }
  }
`;

export const ADD_TO_CART_MUTATION = gql`
  mutation AddToCart($productId: String!, $quantity: Int!) {
    addToCart(productId: $productId, quantity: $quantity) {
      id
      lines {
        productId
        quantity
      }
    }
  }
`;

export const REMOVE_FROM_CART_MUTATION = gql`
  mutation RemoveFromCart($productId: String!) {
    removeFromCart(productId: $productId) {
      id
      lines {
        productId
        quantity
      }
    }
  }
`;

export const CHECKOUT_MUTATION = gql`
  mutation Checkout($idempotencyKey: String!) {
    checkout(idempotencyKey: $idempotencyKey) {
      order {
        id
        totalCents
        subtotalCents
        prorationCreditCents
      }
      agreement {
        id
        status
        contentSnapshot
      }
    }
  }
`;

export const SIGN_AGREEMENT_MUTATION = gql`
  mutation SignAgreement($agreementId: String!, $signatureDataUrl: String!) {
    signAgreement(agreementId: $agreementId, signatureDataUrl: $signatureDataUrl) {
      id
      status
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
