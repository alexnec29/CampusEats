import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';

// Mock ReactDOM
jest.mock('react-dom/client', () => ({
    createRoot: jest.fn(),
}));

// Mock App component
jest.mock('./App', () => () => <div>App Component</div>);

describe('index.tsx', () => {
    const originalGetElementById = document.getElementById;
    const mockRender = jest.fn();

    beforeEach(() => {
        // Reset mocks
        jest.clearAllMocks();
        (ReactDOM.createRoot as jest.Mock).mockReturnValue({
            render: mockRender,
        });
        
        // Mock getElementById
        document.getElementById = jest.fn((id) => {
            if (id === 'root') {
                return document.createElement('div');
            }
            return null;
        });
    });

    afterEach(() => {
        document.getElementById = originalGetElementById;
    });

    it('renders App into root element', () => {
        // We need to require modules to trigger execution
        jest.isolateModules(() => {
            require('./index');
        });

        expect(document.getElementById).toHaveBeenCalledWith('root');
        expect(ReactDOM.createRoot).toHaveBeenCalledTimes(1);
        expect(mockRender).toHaveBeenCalledTimes(1);
        // Checking if render was called with something looking like <App />
        // Since we mocked App, it's <App /> wrapped in StrictMode
        expect(mockRender).toHaveBeenCalledWith(
            expect.objectContaining({
                type: React.StrictMode,
                props: expect.objectContaining({
                    children: expect.objectContaining({
                        type: expect.anything() // App
                    })
                })
            })
        );
    });
});
