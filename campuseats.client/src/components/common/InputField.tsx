import React, { InputHTMLAttributes } from 'react';

interface InputFieldProps extends InputHTMLAttributes<HTMLInputElement> {
    label?: string;
}

export const InputField: React.FC<InputFieldProps> = ({ label, className = '', ...props }) => {
    return (
        <div>
            {label && <label className="block text-sm font-medium mb-1">{label}</label>}
            <input
                className={`w-full p-3 border rounded-lg ${className}`}
                {...props}
            />
        </div>
    );
};
