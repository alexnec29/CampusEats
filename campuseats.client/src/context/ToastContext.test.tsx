import React from 'react';
import { render, screen, waitFor, act } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { ToastProvider, useToast } from './ToastContext';

// Test component to use the ToastContext
const TestComponent: React.FC = () => {
  const { showToast } = useToast();

  return (
    <div>
      <button onClick={() => showToast('Success message', 'success')} data-testid="show-success">
        Show Success
      </button>
      <button onClick={() => showToast('Error message', 'error')} data-testid="show-error">
        Show Error
      </button>
      <button onClick={() => showToast('Info message', 'info')} data-testid="show-info">
        Show Info
      </button>
      <button onClick={() => showToast('Warning message', 'warning')} data-testid="show-warning">
        Show Warning
      </button>
    </div>
  );
};

describe('ToastContext', () => {
  beforeEach(() => {
    jest.useFakeTimers();
  });

  afterEach(() => {
    jest.runOnlyPendingTimers();
    jest.useRealTimers();
  });

  test('throws error when useToast is used outside ToastProvider', () => {
    // Suppress console.error for this test
    const consoleSpy = jest.spyOn(console, 'error').mockImplementation(() => {});

    expect(() => {
      render(<TestComponent />);
    }).toThrow('useToast must be used within a ToastProvider');

    consoleSpy.mockRestore();
  });

  test('renders children without toasts initially', () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    expect(screen.getByTestId('show-success')).toBeInTheDocument();
    expect(screen.queryByText('Success message')).not.toBeInTheDocument();
  });

  test('shows success toast when triggered', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    await userEvent.click(screen.getByTestId('show-success'));

    expect(screen.getByText('Success message')).toBeInTheDocument();
  });

  test('shows error toast with correct styling', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    await userEvent.click(screen.getByTestId('show-error'));

    const toast = screen.getByText('Error message');
    expect(toast).toBeInTheDocument();
    expect(toast.parentElement?.parentElement).toHaveClass('from-red-500');
  });

  test('shows info toast with correct styling', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    await userEvent.click(screen.getByTestId('show-info'));

    const toast = screen.getByText('Info message');
    expect(toast).toBeInTheDocument();
    expect(toast.parentElement?.parentElement).toHaveClass('from-blue-500');
  });

  test('shows warning toast with correct styling', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    await userEvent.click(screen.getByTestId('show-warning'));

    const toast = screen.getByText('Warning message');
    expect(toast).toBeInTheDocument();
    expect(toast.parentElement?.parentElement).toHaveClass('from-yellow-500');
  });

  test('automatically removes toast after 3 seconds', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    await userEvent.click(screen.getByTestId('show-success'));

    expect(screen.getByText('Success message')).toBeInTheDocument();

    // Fast-forward time by 3 seconds
    act(() => {
      jest.advanceTimersByTime(3000);
    });

    await waitFor(() => {
      expect(screen.queryByText('Success message')).not.toBeInTheDocument();
    });
  });

  test('can display multiple toasts simultaneously', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    await userEvent.click(screen.getByTestId('show-success'));
    await userEvent.click(screen.getByTestId('show-error'));
    await userEvent.click(screen.getByTestId('show-info'));

    expect(screen.getByText('Success message')).toBeInTheDocument();
    expect(screen.getByText('Error message')).toBeInTheDocument();
    expect(screen.getByText('Info message')).toBeInTheDocument();
  });

  test('can manually close toast by clicking close button', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    await userEvent.click(screen.getByTestId('show-success'));

    expect(screen.getByText('Success message')).toBeInTheDocument();

    const closeButton = screen.getByText('✕');
    await userEvent.click(closeButton);

    await waitFor(() => {
      expect(screen.queryByText('Success message')).not.toBeInTheDocument();
    });
  });

  test('each toast has unique ID', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    await userEvent.click(screen.getByTestId('show-success'));
    await userEvent.click(screen.getByTestId('show-success'));

    const toasts = screen.getAllByText('Success message');
    expect(toasts).toHaveLength(2);
  });

  test('removes correct toast when multiple toasts exist', async () => {
    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    // Create first toast
    await userEvent.click(screen.getByTestId('show-success'));
    
    // Wait a bit and create second toast
    act(() => {
      jest.advanceTimersByTime(100);
    });
    
    await userEvent.click(screen.getByTestId('show-error'));

    // Both toasts should be visible
    expect(screen.getByText('Success message')).toBeInTheDocument();
    expect(screen.getByText('Error message')).toBeInTheDocument();

    // Advance time by 2.9 seconds - first toast timeout should trigger soon
    act(() => {
      jest.advanceTimersByTime(2900);
    });

    // First toast should be removed after its 3 second timeout
    await waitFor(() => {
      expect(screen.queryByText('Success message')).not.toBeInTheDocument();
    });

    // Second toast should still be visible (100ms buffer remaining)
    expect(screen.getByText('Error message')).toBeInTheDocument();
  });
});
