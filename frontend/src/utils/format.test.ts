import { describe, expect, it } from 'vitest';
import { formatCents, formatDate } from './format';

describe('formatCents', () => {
  it('renders whole dollar amounts with two decimal places', () => {
    expect(formatCents(5900)).toBe('$59.00');
  });

  it('renders cents that are not a round number of dollars', () => {
    expect(formatCents(1499)).toBe('$14.99');
  });

  it('renders zero as $0.00', () => {
    expect(formatCents(0)).toBe('$0.00');
  });
});

describe('formatDate', () => {
  it('renders an ISO timestamp as a short month, day, year', () => {
    expect(formatDate('2026-03-05T00:00:00.000Z')).toBe('Mar 5, 2026');
  });
});
