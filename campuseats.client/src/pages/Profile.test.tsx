import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import Profile from './Profile';
import { AuthProvider } from '../context/AuthContext';
import { ToastProvider } from '../context/ToastContext';
import { LanguageProvider } from '../context/LanguageContext';
import * as apiClientModule from '../utils/apiClient';

jest.mock('../utils/apiClient');
