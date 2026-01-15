import React from 'react';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { ConfirmProvider, useConfirm } from './ConfirmContext';

const TestComponent = () => {
  const { confirm } = useConfirm();
  const [result, setResult] = React.useState('waiting');

  const trigger = async () => {
    const ans = await confirm({ message: 'Sigur?' });
    setResult(ans ? 'yes' : 'no');
  };

  return (
      <div>
        <button onClick={trigger}>Ask</button>
        <div data-testid="result">{result}</div>
      </div>
  );
};

describe('ConfirmContext', () => {
  it('should show dialog and resolve true on confirm', async () => {
    render(
        <ConfirmProvider>
          <TestComponent />
        </ConfirmProvider>
    );

    // Deschide modala
    fireEvent.click(screen.getByText('Ask'));
    expect(screen.getByText('Sigur?')).toBeInTheDocument();

    // Click Confirm
    fireEvent.click(screen.getByText('Confirmă')); // Textul default din componenta ta

    // Verificăm dispariția modalei și rezultatul
    await waitFor(() => {
      expect(screen.queryByText('Sigur?')).not.toBeInTheDocument();
    });
    expect(screen.getByTestId('result')).toHaveTextContent('yes');
  });

  it('should resolve false on cancel', async () => {
    render(
        <ConfirmProvider>
          <TestComponent />
        </ConfirmProvider>
    );

    fireEvent.click(screen.getByText('Ask'));
    fireEvent.click(screen.getByText('Anulează')); // Textul default

    await waitFor(() => {
      expect(screen.queryByText('Sigur?')).not.toBeInTheDocument();
    });
    expect(screen.getByTestId('result')).toHaveTextContent('no');
  });
});