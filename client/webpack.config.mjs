import path from 'path';
import { fileURLToPath } from 'url';
import HtmlWebpackPlugin from 'html-webpack-plugin';
import { TsconfigPathsPlugin } from 'tsconfig-paths-webpack-plugin';
import { compileFunction } from 'vm';
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export default {
    mode: 'development',
    optimization: {
        minimize: false,

    },
    entry: ['./src/main.tsx', './src/App.tsx'],
    output: {
        filename: 'bundle.js',
        path: path.resolve(__dirname, '../wwwroot/dist'),
//        publicPath: '/',
    },
    resolve: {
        extensions: ['.js', '.jsx', '.ts', '.tsx'],
        plugins: [
            new TsconfigPathsPlugin({})
        ]
    },
    module: {
        rules: [
            {
                test: /\.(ts|tsx)$/,
                use: [{
                    loader: 'ts-loader',
                    options: {
                        compilerOptions: { noEmit: false },
                    }
                }],
                exclude: /node_modules/,
            },
            {
                test: /\.css$/,
                use: ['style-loader', 'css-loader'],
            },
        ],
    },
    plugins: [
        new HtmlWebpackPlugin({
            template: './src/index.html',
            filename: '../index.html',
            inject: 'body',
        }),
    ],
    devServer: {
        static: {
            directory: path.join(__dirname, '../wwwroot/dist'),
        },
        compress: true,
        port: 9000,
        historyApiFallback: true,
    },
    devtool: 'source-map',
};