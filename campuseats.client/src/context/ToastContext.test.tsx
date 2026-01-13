import React from 'react';
import { render, screen, act } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { ToastProvider, useToast } from './ToastContext';

// Helper component
const TestComponent = () => {
  const { showToast } = useToast();
  return (
      <div>
        <button onClick={() => showToast('Success message', 'success')}>Show Success</button>
        <button onClick={() => showToast('Error message', 'error')}>Show Error</button>
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

  it('should display success toast and apply correct styles', async () => {
    const user = userEvent.setup({ delay: null });
    render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
    );

    await user.click(screen.getByText('Show Success'));

    // Găsim textul mesajului
    const messageElement = screen.getByText('Success message');
    expect(messageElement).toBeInTheDocument();

    // Navigăm 2 nivele în sus: <p> -> <div flex> -> <div container colorat>
    // SAU folosim closest cu o clasă comună, de ex 'shadow-lg'
    const toastContainer = messageElement.closest('.shadow-lg');

    expect(toastContainer).toHaveClass('bg-gradient-to-r', 'from-green-500', 'to-emerald-600');
  });

  it('should display error toast and apply correct styles', async () => {
    const user = userEvent.setup({ delay: null });
    render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
    );

    await user.click(screen.getByText('Show Error'));

    const messageElement = screen.getByText('Error message');
    const toastContainer = messageElement.closest('.shadow-lg');

    expect(toastContainer).toHaveClass('bg-gradient-to-r', 'from-red-500', 'to-pink-600');
  });

  it('should automatically remove toast after 3 seconds', async () => {
    const user = userEvent.setup({ delay: null });
    render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
    );

    await user.click(screen.getByText('Show Success'));
    expect(screen.getByText('Success message')).toBeInTheDocument();

    // Avansăm timpul cu 3 secunde
    act(() => {
      jest.advanceTimersByTime(3000);
    });

    expect(screen.queryByText('Success message')).not.toBeInTheDocument();
  });

  it('should allow manual removal', async () => {
    const user = userEvent.setup({ delay: null });
    render(
        <ToastProvider>
          <TestComponent />
        </ToastProvider>
    );

    await user.click(screen.getByText('Show Success'));
    const closeButton = screen.getByText('✕'); // sau getByRole('button', { name: /✕/i })

    await user.click(closeButton);
    expect(screen.queryByText('Success message')).not.toBeInTheDocument();
  });
});