import 'bootstrap/dist/css/bootstrap.min.css';
import './App.css'; // Assuming you have related styles
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';

const container = document.getElementById('root') as HTMLElement;
const root = ReactDOM.createRoot(container);
root.render(
    <React.StrictMode>
    <h1>Hello World, root</h1>
        <App />
    </React.StrictMode>
);
console.log("hellow world")