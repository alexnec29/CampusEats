import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { ToastProvider, useToast } from './ToastContext';

jest.useFakeTimers();

const TestComponent = () => {
  const { showToast } = useToast();
  
  return (
    <div>
      <button onClick={() => showToast('Success message', 'success')}>Show Success</button>
      <button onClick={() => showToast('Error message', 'error')}>Show Error</button>
      <button onClick={() => showToast('Info message', 'info')}>Show Info</button>
      <button onClick={() => showToast('Warning message', 'warning')}>Show Warning</button>
    </div>
  );
};

describe('ToastContext', () => {
  beforeEach(() => {
    jest.clearAllTimers();
  });

  afterEach(() => {
    jest.clearAllTimers();
  });

  it('should throw error when useToast is used outside ToastProvider', () => {
    const consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});
    
    expect(() => render(<TestComponent />)).toThrow('useToast must be used within a ToastProvider');
    
    consoleError.mockRestore();
  });

  it('should display success toast when showToast is called', async () => {
    const user = userEvent.setup({ delay: null });

    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    const button = screen.getByRole('button', { name: /show success/i });
    await user.click(button);

    expect(screen.getByText('Success message')).toBeInTheDocument();
  });

  it('should display error toast', async () => {
    const user = userEvent.setup({ delay: null });

    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    const button = screen.getByRole('button', { name: /show error/i });
    await user.click(button);

    expect(screen.getByText('Error message')).toBeInTheDocument();
  });

  it('should display info toast', async () => {
    const user = userEvent.setup({ delay: null });

    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    const button = screen.getByRole('button', { name: /show info/i });
    await user.click(button);

    expect(screen.getByText('Info message')).toBeInTheDocument();
  });

  it('should display warning toast', async () => {
    const user = userEvent.setup({ delay: null });

    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    const button = screen.getByRole('button', { name: /show warning/i });
    await user.click(button);

    expect(screen.getByText('Warning message')).toBeInTheDocument();
  });

  it('should automatically remove toast after 3 seconds', async () => {
    const user = userEvent.setup({ delay: null });

    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    const button = screen.getByRole('button', { name: /show success/i });
    await user.click(button);

    expect(screen.getByText('Success message')).toBeInTheDocument();

    jest.advanceTimersByTime(3000);

    await waitFor(() => {
      expect(screen.queryByText('Success message')).not.toBeInTheDocument();
    });
  });

  it('should allow manual removal of toast by clicking close button', async () => {
    const user = userEvent.setup({ delay: null });

    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    const button = screen.getByRole('button', { name: /show success/i });
    await user.click(button);

    expect(screen.getByText('Success message')).toBeInTheDocument();

    const closeButton = screen.getByRole('button', { name: /✕/i });
    await user.click(closeButton);

    expect(screen.queryByText('Success message')).not.toBeInTheDocument();
  });

  it('should display multiple toasts simultaneously', async () => {
    const user = userEvent.setup({ delay: null });

    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    const successButton = screen.getByRole('button', { name: /show success/i });
    const errorButton = screen.getByRole('button', { name: /show error/i });

    await user.click(successButton);
    await user.click(errorButton);

    expect(screen.getByText('Success message')).toBeInTheDocument();
    expect(screen.getByText('Error message')).toBeInTheDocument();
  });

  it('should apply correct CSS classes for success toast', async () => {
    const user = userEvent.setup({ delay: null });

    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    const button = screen.getByRole('button', { name: /show success/i });
    await user.click(button);

    const toast = screen.getByText('Success message').closest('div');
    expect(toast).toHaveClass('bg-gradient-to-r', 'from-green-500', 'to-emerald-600');
  });

  it('should apply correct CSS classes for error toast', async () => {
    const user = userEvent.setup({ delay: null });

    render(
      <ToastProvider>
        <TestComponent />
      </ToastProvider>
    );

    const button = screen.getByRole('button', { name: /show error/i });
    await user.click(button);

    const toast = screen.getByText('Error message').closest('div');
    expect(toast).toHaveClass('bg-gradient-to-r', 'from-red-500', 'to-pink-600');
  });
});
