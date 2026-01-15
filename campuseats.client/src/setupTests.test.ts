// This test file ensures that setupTests.ts is included in coverage reports
// as it often only contains imports and no logic.

describe('setupTests', () => {
    it('executes setupTests successfully', () => {
        // Since setupTests is configured in create-react-app to run before tests,
        // if we are here, it has already run.
        // However, we can also try to require it to ensure line coverage if necessary.
        jest.isolateModules(() => {
            require('./setupTests');
        });
        // We just assert true here, the meaningful part is that it didn't crash.
        expect(true).toBe(true);
    });

    it('extends jest with testing-library matchers', () => {
        // Verify that a matcher from @testing-library/jest-dom is available
        expect(expect(document.createElement('div')).toBeInTheDocument).toBeDefined();
    });
});
