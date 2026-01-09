# Frontend Integration Guide: Allergen System

## Overview
This guide shows how to integrate the allergen management system into the frontend.

## Backend API (Already Implemented)

### Get All Allergens
```typescript
GET /api/allergens
Authorization: Bearer <token> (AllRoles)

Response:
[
  {
    "id": 1,
    "name": "Peanuts"
  },
  {
    "id": 2,
    "name": "Gluten"
  },
  {
    "id": 3,
    "name": "Dairy"
  }
]
```

### Get Allergen by ID
```typescript
GET /api/allergens/{id}
Authorization: Bearer <token> (AllRoles)

Response:
{
  "id": 1,
  "name": "Peanuts"
}
```

### Create Allergen (Kitchen Staff Only)
```typescript
POST /api/allergens
Authorization: Bearer <token> (Kitchen role)
Content-Type: application/json

Body:
{
  "name": "Shellfish"
}

Response:
{
  "id": 4,
  "name": "Shellfish"
}
```

### Delete Allergen (Kitchen Staff Only)
```typescript
DELETE /api/allergens/{id}
Authorization: Bearer <token> (Kitchen role)

Response: 204 No Content
```

## Frontend Implementation

### Step 1: Add Allergen Types
Update `src/types/index.ts`:

```typescript
export interface Allergen {
  id: number;
  name: string;
}

export interface MenuItem {
  id: number;
  name: string;
  description: string;
  price: number;
  category: MenuCategory;
  imageUrl?: string;
  isAvailable: boolean;
  allergens?: Allergen[];  // Add this
}
```

### Step 2: Create Allergen Service
Create `src/services/allergenService.ts`:

```typescript
import { apiClient } from '../utils/apiClient';

export interface Allergen {
  id: number;
  name: string;
}

export const allergenService = {
  async getAll(): Promise<Allergen[]> {
    try {
      const response = await apiClient('/api/allergens');
      if (response.ok) {
        return await response.json();
      }
      return [];
    } catch (error) {
      console.error('Error fetching allergens:', error);
      return [];
    }
  },

  async getById(id: number): Promise<Allergen | null> {
    try {
      const response = await apiClient(`/api/allergens/${id}`);
      if (response.ok) {
        return await response.json();
      }
      return null;
    } catch (error) {
      console.error('Error fetching allergen:', error);
      return null;
    }
  },

  async create(name: string): Promise<Allergen | null> {
    try {
      const response = await apiClient('/api/allergens', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name })
      });
      if (response.ok) {
        return await response.json();
      }
      return null;
    } catch (error) {
      console.error('Error creating allergen:', error);
      return null;
    }
  },

  async delete(id: number): Promise<boolean> {
    try {
      const response = await apiClient(`/api/allergens/${id}`, {
        method: 'DELETE'
      });
      return response.ok;
    } catch (error) {
      console.error('Error deleting allergen:', error);
      return false;
    }
  }
};
```

### Step 3: Create Allergen Badge Component
Create `src/components/AllergenBadge.tsx`:

```typescript
import React from 'react';
import { Allergen } from '../services/allergenService';

interface AllergenBadgeProps {
  allergen: Allergen;
  size?: 'sm' | 'md' | 'lg';
}

export const AllergenBadge: React.FC<AllergenBadgeProps> = ({ 
  allergen, 
  size = 'md' 
}) => {
  const sizeClasses = {
    sm: 'text-xs px-2 py-0.5',
    md: 'text-sm px-2 py-1',
    lg: 'text-base px-3 py-1.5'
  };

  return (
    <span className={`inline-flex items-center gap-1 bg-yellow-100 text-yellow-800 rounded-full font-medium ${sizeClasses[size]}`}>
      ⚠️ {allergen.name}
    </span>
  );
};

interface AllergenListProps {
  allergens: Allergen[];
  maxDisplay?: number;
}

export const AllergenList: React.FC<AllergenListProps> = ({ 
  allergens, 
  maxDisplay = 3 
}) => {
  if (!allergens || allergens.length === 0) {
    return null;
  }

  const displayAllergens = allergens.slice(0, maxDisplay);
  const remaining = allergens.length - maxDisplay;

  return (
    <div className="flex flex-wrap gap-1">
      {displayAllergens.map(allergen => (
        <AllergenBadge key={allergen.id} allergen={allergen} size="sm" />
      ))}
      {remaining > 0 && (
        <span className="text-xs text-gray-500 self-center">
          +{remaining} more
        </span>
      )}
    </div>
  );
};
```

### Step 4: Update Menu Item Display
Modify `src/pages/Menu.tsx` to show allergens:

```typescript
import React, { useEffect, useState } from 'react';
import { apiClient } from '../utils/apiClient';
import { MenuItem, Order, OrderStatus } from '../types';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useConfirm } from '../context/ConfirmContext';
import { useNavigate } from 'react-router-dom';
import { AllergenList } from '../components/AllergenBadge';

const Menu: React.FC = () => {
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [allergenFilter, setAllergenFilter] = useState<number[]>([]);
  const { isAuthenticated, userRole } = useAuth();
  const { showToast } = useToast();
  const { confirm } = useConfirm();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchMenu = async () => {
      try {
        const response = await apiClient('/api/menu-items');
        if (response.ok) {
          const data = await response.json();
          setMenuItems(data);
        }
      } catch (error) {
        console.error('Error fetching menu:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchMenu();
  }, []);

  const filteredItems = allergenFilter.length > 0
    ? menuItems.filter(item => 
        !item.allergens?.some(allergen => allergenFilter.includes(allergen.id))
      )
    : menuItems;

  // ... rest of the component

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-4xl font-bold mb-8">Menu</h1>

      {/* Allergen Filter Section */}
      {/* Add this section to allow filtering by allergens */}
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredItems.map((item) => (
          <div key={item.id} className="bg-white rounded-lg shadow-md overflow-hidden">
            {item.imageUrl && (
              <img 
                src={item.imageUrl} 
                alt={item.name}
                className="w-full h-48 object-cover"
              />
            )}
            <div className="p-4">
              <h3 className="text-xl font-semibold mb-2">{item.name}</h3>
              <p className="text-gray-600 mb-3">{item.description}</p>
              
              {/* Display allergens */}
              {item.allergens && item.allergens.length > 0 && (
                <div className="mb-3">
                  <p className="text-xs text-gray-500 mb-1">Contains:</p>
                  <AllergenList allergens={item.allergens} />
                </div>
              )}
              
              <div className="flex justify-between items-center">
                <span className="text-2xl font-bold text-green-600">
                  ${item.price.toFixed(2)}
                </span>
                <button
                  onClick={() => addToOrder(item)}
                  disabled={!item.isAvailable}
                  className={`px-4 py-2 rounded transition-colors ${
                    item.isAvailable
                      ? 'bg-green-600 hover:bg-green-700 text-white'
                      : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                  }`}
                >
                  {item.isAvailable ? 'Add to Order' : 'Unavailable'}
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Menu;
```

### Step 5: Add Allergen Management for Kitchen Staff
Create `src/pages/AllergenManagement.tsx`:

```typescript
import React, { useEffect, useState } from 'react';
import { allergenService, Allergen } from '../services/allergenService';
import { useToast } from '../context/ToastContext';
import { useConfirm } from '../context/ConfirmContext';

const AllergenManagement: React.FC = () => {
  const [allergens, setAllergens] = useState<Allergen[]>([]);
  const [newAllergenName, setNewAllergenName] = useState('');
  const [loading, setLoading] = useState(true);
  const { showToast } = useToast();
  const { confirm } = useConfirm();

  useEffect(() => {
    fetchAllergens();
  }, []);

  const fetchAllergens = async () => {
    setLoading(true);
    const data = await allergenService.getAll();
    setAllergens(data);
    setLoading(false);
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!newAllergenName.trim()) {
      showToast('Please enter an allergen name', 'error');
      return;
    }

    const result = await allergenService.create(newAllergenName.trim());
    if (result) {
      showToast('Allergen created successfully', 'success');
      setNewAllergenName('');
      fetchAllergens();
    } else {
      showToast('Failed to create allergen', 'error');
    }
  };

  const handleDelete = async (allergen: Allergen) => {
    const confirmed = await confirm(
      `Are you sure you want to delete "${allergen.name}"?`,
      'This action cannot be undone.'
    );

    if (confirmed) {
      const success = await allergenService.delete(allergen.id);
      if (success) {
        showToast('Allergen deleted successfully', 'success');
        fetchAllergens();
      } else {
        showToast('Failed to delete allergen', 'error');
      }
    }
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-4xl font-bold mb-8">Allergen Management</h1>

      {/* Create New Allergen */}
      <div className="bg-white rounded-lg shadow-md p-6 mb-8">
        <h2 className="text-2xl font-semibold mb-4">Add New Allergen</h2>
        <form onSubmit={handleCreate} className="flex gap-4">
          <input
            type="text"
            value={newAllergenName}
            onChange={(e) => setNewAllergenName(e.target.value)}
            placeholder="Allergen name (e.g., Peanuts, Gluten)"
            className="flex-1 px-4 py-2 border border-gray-300 rounded-md focus:ring-green-500 focus:border-green-500"
          />
          <button
            type="submit"
            className="px-6 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition-colors"
          >
            Add Allergen
          </button>
        </form>
      </div>

      {/* Allergen List */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-2xl font-semibold mb-4">Existing Allergens</h2>
        
        {loading ? (
          <div className="text-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-green-600 mx-auto"></div>
          </div>
        ) : allergens.length === 0 ? (
          <p className="text-gray-500 text-center py-8">
            No allergens defined yet. Add one above.
          </p>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {allergens.map((allergen) => (
              <div
                key={allergen.id}
                className="flex items-center justify-between p-4 border border-gray-200 rounded-md hover:bg-gray-50"
              >
                <div className="flex items-center gap-2">
                  <span className="text-2xl">⚠️</span>
                  <span className="font-medium">{allergen.name}</span>
                </div>
                <button
                  onClick={() => handleDelete(allergen)}
                  className="text-red-600 hover:text-red-800 transition-colors"
                  title="Delete allergen"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default AllergenManagement;
```

### Step 6: Update AddMenuItem to Include Allergens
Modify `src/pages/AddMenuItem.tsx` to allow selecting allergens:

```typescript
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiClient } from '../utils/apiClient';
import { MenuCategory } from '../types';
import { useToast } from '../context/ToastContext';
import { allergenService, Allergen } from '../services/allergenService';

const AddMenuItem: React.FC = () => {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [allergens, setAllergens] = useState<Allergen[]>([]);
  const [selectedAllergenIds, setSelectedAllergenIds] = useState<number[]>([]);
  
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    price: '',
    category: MenuCategory.Breakfast,
    imageUrl: '',
    isAvailable: true
  });

  useEffect(() => {
    const fetchAllergens = async () => {
      const data = await allergenService.getAll();
      setAllergens(data);
    };
    fetchAllergens();
  }, []);

  const toggleAllergen = (allergenId: number) => {
    setSelectedAllergenIds(prev => 
      prev.includes(allergenId)
        ? prev.filter(id => id !== allergenId)
        : [...prev, allergenId]
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      const payload = {
        ...formData,
        price: parseFloat(formData.price),
        category: Number(formData.category),
        allergenIds: selectedAllergenIds  // Add this
      };

      const response = await apiClient('/api/menu-items', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });

      if (response.ok) {
        showToast('Menu item added successfully!', 'success');
        navigate('/menu');
      } else {
        showToast('Failed to add menu item', 'error');
      }
    } catch (error) {
      showToast('Error adding menu item', 'error');
    }
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-4xl font-bold mb-8">Add Menu Item</h1>
      
      <form onSubmit={handleSubmit} className="max-w-2xl bg-white rounded-lg shadow-md p-6">
        {/* ... existing form fields ... */}
        
        {/* Allergens Section */}
        <div className="mb-4">
          <label className="block text-gray-700 font-medium mb-2">
            Allergens
          </label>
          <div className="border border-gray-300 rounded-md p-4 space-y-2">
            {allergens.length === 0 ? (
              <p className="text-gray-500 text-sm">No allergens defined yet</p>
            ) : (
              allergens.map(allergen => (
                <label key={allergen.id} className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={selectedAllergenIds.includes(allergen.id)}
                    onChange={() => toggleAllergen(allergen.id)}
                    className="w-4 h-4 text-green-600 rounded focus:ring-green-500"
                  />
                  <span className="text-gray-700">⚠️ {allergen.name}</span>
                </label>
              ))
            )}
          </div>
          <p className="text-sm text-gray-500 mt-1">
            Select all allergens contained in this menu item
          </p>
        </div>

        {/* ... rest of form ... */}
      </form>
    </div>
  );
};

export default AddMenuItem;
```

## Key Features to Implement

### 1. Menu Item Display
- Show allergen badges on each menu item
- Visual warning with ⚠️ icon
- Compact display for multiple allergens

### 2. Allergen Filtering
- Allow users to filter menu by allergens
- Hide items containing specific allergens
- Useful for customers with allergies

### 3. Kitchen Management
- Add/remove allergens (Kitchen staff only)
- Assign allergens to menu items
- View all allergens in the system

### 4. User Safety
- Clear visual indicators
- Prominent allergen warnings
- Easy-to-understand labeling

## Testing

1. **View allergens**: Navigate to menu and see allergen badges
2. **Add allergen**: Kitchen staff can create new allergens
3. **Assign to items**: When creating menu items, select allergens
4. **Filter menu**: Users can filter menu by excluding allergens
5. **Delete allergen**: Kitchen staff can remove unused allergens

## Notes

- Allergen endpoints require authentication
- Kitchen role required for create/delete operations
- All users can view allergens
- Consider adding allergen preferences to user profiles
