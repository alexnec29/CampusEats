import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { ConfirmProvider, useConfirm } from './ConfirmContext';

// Test component to use the ConfirmContext
const TestComponent: React.FC = () => {
  const { confirm } = useConfirm();
  const [result, setResult] = React.useState<string>('');

  const handleConfirm = async () => {
    const confirmed = await confirm({
      title: 'Test Title',
      message: 'Test message',
      confirmText: 'Yes',
      cancelText: 'No',
    });
    setResult(confirmed ? 'confirmed' : 'cancelled');
  };

  const handleDangerConfirm = async () => {
    const confirmed = await confirm({
      message: 'Delete this item?',
      type: 'danger',
    });
    setResult(confirmed ? 'deleted' : 'not-deleted');
  };

  return (
    <div>
      <button onClick={handleConfirm} data-testid="trigger-confirm">
        Trigger Confirm
      </button>
      <button onClick={handleDangerConfirm} data-testid="trigger-danger">
        Trigger Danger
      </button>
      <div data-testid="result">{result}</div>
    </div>
  );
};

describe('ConfirmContext', () => {
  test('throws error when useConfirm is used outside ConfirmProvider', () => {
    // Suppress console.error for this test
    const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

    expect(() => {
      render(<TestComponent />);
    }).toThrow('useConfirm must be used within a ConfirmProvider');

    consoleSpy.mockRestore();
  });

  test('renders children without dialog initially', () => {
    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    expect(screen.getByTestId('trigger-confirm')).toBeInTheDocument();
    expect(screen.queryByText('Test message')).not.toBeInTheDocument();
  });

  test('shows confirmation dialog when triggered', async () => {
    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    await userEvent.click(screen.getByTestId('trigger-confirm'));

    expect(screen.getByText('Test Title')).toBeInTheDocument();
    expect(screen.getByText('Test message')).toBeInTheDocument();
    expect(screen.getByText('Yes')).toBeInTheDocument();
    expect(screen.getByText('No')).toBeInTheDocument();
  });

  test('resolves true when confirm button is clicked', async () => {
    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    await userEvent.click(screen.getByTestId('trigger-confirm'));

    const confirmButton = screen.getByText('Yes');
    await userEvent.click(confirmButton);

    await waitFor(() => {
      expect(screen.getByTestId('result')).toHaveTextContent('confirmed');
    });

    // Dialog should be closed
    expect(screen.queryByText('Test message')).not.toBeInTheDocument();
  });

  test('resolves false when cancel button is clicked', async () => {
    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    await userEvent.click(screen.getByTestId('trigger-confirm'));

    const cancelButton = screen.getByText('No');
    await userEvent.click(cancelButton);

    await waitFor(() => {
      expect(screen.getByTestId('result')).toHaveTextContent('cancelled');
    });

    // Dialog should be closed
    expect(screen.queryByText('Test message')).not.toBeInTheDocument();
  });

  test('uses default title when not provided', async () => {
    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    await userEvent.click(screen.getByTestId('trigger-danger'));

    expect(screen.getByText('Confirmare')).toBeInTheDocument();
  });

  test('uses default button texts when not provided', async () => {
    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    await userEvent.click(screen.getByTestId('trigger-danger'));

    expect(screen.getByText('Anulează')).toBeInTheDocument();
    expect(screen.getByText('Confirmă')).toBeInTheDocument();
  });

  test('applies danger styling when type is danger', async () => {
    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    await userEvent.click(screen.getByTestId('trigger-danger'));

    const confirmButton = screen.getByText('Confirmă');
    expect(confirmButton).toHaveClass('from-red-500');
  });

  test('applies info styling when type is not danger', async () => {
    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    await userEvent.click(screen.getByTestId('trigger-confirm'));

    const confirmButton = screen.getByText('Yes');
    expect(confirmButton).toHaveClass('from-blue-600');
  });

  test('handles multiple sequential confirmations', async () => {
    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    // First confirmation
    await userEvent.click(screen.getByTestId('trigger-confirm'));
    await userEvent.click(screen.getByText('Yes'));

    await waitFor(() => {
      expect(screen.getByTestId('result')).toHaveTextContent('confirmed');
    });

    // Second confirmation
    await userEvent.click(screen.getByTestId('trigger-danger'));
    await userEvent.click(screen.getByText('Anulează'));

    await waitFor(() => {
      expect(screen.getByTestId('result')).toHaveTextContent('not-deleted');
    });
  });
});
