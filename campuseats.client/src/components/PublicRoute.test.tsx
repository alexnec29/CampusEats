import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import PublicRoute from './PublicRoute';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');
