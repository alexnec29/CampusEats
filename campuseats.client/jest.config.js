module.exports = {
  transformIgnorePatterns: [
    'node_modules/(?!(react-router|@?react-router.*|@remix-run)/)'
  ],
  moduleNameMapper: {
    '^react-router-dom$': '<rootDir>/src/__mocks__/react-router-dom.tsx'
  }
};
