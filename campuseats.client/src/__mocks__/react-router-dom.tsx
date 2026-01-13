import React from 'react';

export const useNavigate = () => jest.fn();
export const useLocation = () => ({ pathname: '/', state: null });
export const Link = ({ children, to, ...props }: any) => <a href={to} {...props}>{children}</a>;
export const Navigate = ({ to }: any) => <div>Navigating to {to}</div>;
export const BrowserRouter = ({ children }: any) => <div>{children}</div>;
export const Routes = ({ children }: any) => <div>{children}</div>;
export const Route = ({ element }: any) => <div>{element}</div>;
