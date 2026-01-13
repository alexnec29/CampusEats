import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import KitchenOrders from './KitchenOrders';
import { AuthProvider } from '../context/AuthContext';
import { ToastProvider } from '../context/ToastContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');
