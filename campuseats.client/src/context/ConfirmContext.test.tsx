import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { ConfirmProvider, useConfirm } from './ConfirmContext';

const TestComponent = () => {
  const { confirm } = useConfirm();
  const [result, setResult] = React.useState<string>('');
  
  const handleConfirm = async () => {
    const confirmed = await confirm({ message: 'Are you sure?' });
    setResult(confirmed ? 'confirmed' : 'cancelled');
  };

  const handleDangerConfirm = async () => {
    const confirmed = await confirm({
      title: 'Delete Item',
      message: 'This action cannot be undone',
      confirmText: 'Delete',
      cancelText: 'Keep',
      type: 'danger',
    });
    setResult(confirmed ? 'deleted' : 'kept');
  };
  
  return (
    <div>
      <button onClick={handleConfirm}>Request Confirm</button>
      <button onClick={handleDangerConfirm}>Request Danger Confirm</button>
      <div data-testid="result">{result}</div>
    </div>
  );
};

describe('ConfirmContext', () => {
  it('should throw error when useConfirm is used outside ConfirmProvider', () => {
    const consoleError = jest.spyOn(console, 'error').mockImplementation(() => {});
    
    expect(() => render(<TestComponent />)).toThrow('useConfirm must be used within a ConfirmProvider');
    
    consoleError.mockRestore();
  });

  it('should display confirmation dialog when confirm is called', async () => {
    const user = userEvent.setup();

    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    const button = screen.getByRole('button', { name: /request confirm/i });
    await user.click(button);

    expect(screen.getByText('Are you sure?')).toBeInTheDocument();
  });

  it('should resolve with true when confirm button is clicked', async () => {
    const user = userEvent.setup();

    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    const requestButton = screen.getByRole('button', { name: /request confirm/i });
    await user.click(requestButton);

    const confirmButton = screen.getByRole('button', { name: /confirmă/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(screen.getByTestId('result')).toHaveTextContent('confirmed');
    });
  });

  it('should resolve with false when cancel button is clicked', async () => {
    const user = userEvent.setup();

    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    const requestButton = screen.getByRole('button', { name: /request confirm/i });
    await user.click(requestButton);

    const cancelButton = screen.getByRole('button', { name: /anulează/i });
    await user.click(cancelButton);

    await waitFor(() => {
      expect(screen.getByTestId('result')).toHaveTextContent('cancelled');
    });
  });

  it('should display custom title and message', async () => {
    const user = userEvent.setup();

    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    const button = screen.getByRole('button', { name: /request danger confirm/i });
    await user.click(button);

    expect(screen.getByText('Delete Item')).toBeInTheDocument();
    expect(screen.getByText('This action cannot be undone')).toBeInTheDocument();
  });

  it('should display custom button texts', async () => {
    const user = userEvent.setup();

    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    const button = screen.getByRole('button', { name: /request danger confirm/i });
    await user.click(button);

    expect(screen.getByRole('button', { name: /delete/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /keep/i })).toBeInTheDocument();
  });

  it('should apply danger styling for danger type', async () => {
    const user = userEvent.setup();

    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    const button = screen.getByRole('button', { name: /request danger confirm/i });
    await user.click(button);

    const confirmButton = screen.getByRole('button', { name: /delete/i });
    expect(confirmButton).toHaveClass('from-red-500', 'to-pink-600');
  });

  it('should apply info styling for default type', async () => {
    const user = userEvent.setup();

    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    const button = screen.getByRole('button', { name: /request confirm/i });
    await user.click(button);

    const confirmButton = screen.getByRole('button', { name: /confirmă/i });
    expect(confirmButton).toHaveClass('from-blue-600', 'to-purple-600');
  });

  it('should close dialog after confirmation', async () => {
    const user = userEvent.setup();

    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    const button = screen.getByRole('button', { name: /request confirm/i });
    await user.click(button);

    expect(screen.getByText('Are you sure?')).toBeInTheDocument();

    const confirmButton = screen.getByRole('button', { name: /confirmă/i });
    await user.click(confirmButton);

    await waitFor(() => {
      expect(screen.queryByText('Are you sure?')).not.toBeInTheDocument();
    });
  });

  it('should display default title when not provided', async () => {
    const user = userEvent.setup();

    render(
      <ConfirmProvider>
        <TestComponent />
      </ConfirmProvider>
    );

    const button = screen.getByRole('button', { name: /request confirm/i });
    await user.click(button);

    expect(screen.getByText('Confirmare')).toBeInTheDocument();
  });
});
