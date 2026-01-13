import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import AdminDashboard from './AdminDashboard';
import { AuthProvider } from '../context/AuthContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');
