import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import '@testing-library/jest-dom';
import { MemoryRouter } from 'react-router-dom';
import Landing from './Landing';

const renderLanding = () => {
  return render(
    <MemoryRouter>
      <Landing />
    </MemoryRouter>
  );
};

describe('Landing Page', () => {
  test('renders the main heading with CampusEats', () => {
    renderLanding();
    
    expect(screen.getByText('CampusEats')).toBeInTheDocument();
    expect(screen.getByText(/Bine ai venit la/i)).toBeInTheDocument();
  });

  test('displays login button with link to /login', () => {
    renderLanding();
    
    const loginButtons = screen.getAllByText('Login');
    expect(loginButtons.length).toBeGreaterThan(0);
    
    const loginLink = loginButtons[0].closest('a');
    expect(loginLink).toHaveAttribute('href', '/login');
  });

  test('displays register button with link to /register', () => {
    renderLanding();
    
    const registerButton = screen.getByText(/Înregistrează-te Gratuit/i);
    expect(registerButton).toBeInTheDocument();
    
    const registerLink = registerButton.closest('a');
    expect(registerLink).toHaveAttribute('href', '/register');
  });

  test('displays all four feature cards', () => {
    renderLanding();
    
    expect(screen.getByText('Meniu Variat')).toBeInTheDocument();
    expect(screen.getByText('Livrare Rapidă')).toBeInTheDocument();
    expect(screen.getByText('Comandă Ușor')).toBeInTheDocument();
    expect(screen.getByText('Puncte Loialitate')).toBeInTheDocument();
  });

  test('displays "How it works" section with three steps', () => {
    renderLanding();
    
    expect(screen.getByText('Cum funcționează?')).toBeInTheDocument();
    expect(screen.getByText('Înregistrează-te')).toBeInTheDocument();
    expect(screen.getByText('Alege Mâncarea')).toBeInTheDocument();
    expect(screen.getByText('Primește Comanda')).toBeInTheDocument();
  });

  test('displays language selector with RO and EN buttons', () => {
    renderLanding();
    
    const roButton = screen.getByText('RO');
    const enButton = screen.getByText('EN');
    
    expect(roButton).toBeInTheDocument();
    expect(enButton).toBeInTheDocument();
  });

  test('switches language to English when EN button is clicked', async () => {
    renderLanding();
    
    const enButton = screen.getByText('EN');
    await userEvent.click(enButton);
    
    expect(screen.getByText('Welcome to')).toBeInTheDocument();
    expect(screen.getByText('Varied Menu')).toBeInTheDocument();
    expect(screen.getByText('Fast Delivery')).toBeInTheDocument();
    expect(screen.getByText('How does it work?')).toBeInTheDocument();
  });

  test('switches back to Romanian when RO button is clicked', async () => {
    renderLanding();
    
    const enButton = screen.getByText('EN');
    await userEvent.click(enButton);
    
    expect(screen.getByText('Welcome to')).toBeInTheDocument();
    
    const roButton = screen.getByText('RO');
    await userEvent.click(roButton);
    
    expect(screen.getByText(/Bine ai venit la/i)).toBeInTheDocument();
    expect(screen.getByText('Meniu Variat')).toBeInTheDocument();
  });

  test('RO button has active styling when Romanian is selected', () => {
    renderLanding();
    
    const roButton = screen.getByText('RO');
    expect(roButton).toHaveClass('bg-blue-600');
    expect(roButton).toHaveClass('text-white');
  });

  test('EN button has active styling when English is selected', async () => {
    renderLanding();
    
    const enButton = screen.getByText('EN');
    await userEvent.click(enButton);
    
    expect(enButton).toHaveClass('bg-blue-600');
    expect(enButton).toHaveClass('text-white');
  });

  test('displays subtitle in Romanian by default', () => {
    renderLanding();
    
    expect(
      screen.getByText(/Comandă mâncare delicioasă direct din campus, rapid și simplu!/i)
    ).toBeInTheDocument();
  });

  test('displays English subtitle after switching language', async () => {
    renderLanding();
    
    const enButton = screen.getByText('EN');
    await userEvent.click(enButton);
    
    expect(
      screen.getByText(/Order delicious food directly from campus, fast and simple!/i)
    ).toBeInTheDocument();
  });

  test('displays CTA section', () => {
    renderLanding();
    
    expect(screen.getByText('Gata să începi?')).toBeInTheDocument();
    expect(screen.getByText(/Înregistrează-te acum și bucură-te de mâncare delicioasă!/i)).toBeInTheDocument();
  });

  test('displays English CTA after language switch', async () => {
    renderLanding();
    
    const enButton = screen.getByText('EN');
    await userEvent.click(enButton);
    
    expect(screen.getByText('Ready to start?')).toBeInTheDocument();
    expect(screen.getByText(/Register now and enjoy delicious food!/i)).toBeInTheDocument();
    expect(screen.getByText('Sign Up Free')).toBeInTheDocument();
  });

  test('step numbers are displayed correctly', () => {
    renderLanding();
    
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
  });
});
