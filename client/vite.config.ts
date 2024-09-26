import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
    plugins: [react()],
    server: {
        proxy: {
            '/chat': 'http://localhost:5000',
        },
    },
    build: {
        outDir: resolve(__dirname, '../wwwroot/dist'), // Correct path usage and ensure it's resolving correctly
        manifest: true, // Ensure manifest.json generation
        rollupOptions: {
            input: {
                main: resolve(__dirname, 'index.html'), // Ensure it's pointing to the correct index.html
            },
            output: {
                entryFileNames: 'js/[name].[hash].js',
                chunkFileNames: 'js/[name].[hash].js',
                assetFileNames: ({ name }) => {
                    if (/\.(css)$/.test(name ?? '')) {
                        return 'css/[name].[hash][extname]';
                    }
                    return 'assets/[name].[hash][extname]';
                }
            }
        }
    }
});