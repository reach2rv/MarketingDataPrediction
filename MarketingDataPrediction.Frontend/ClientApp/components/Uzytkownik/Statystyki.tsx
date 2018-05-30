﻿import * as React from "react";
import * as ReactDOM from "react-dom";
import { LineChart, XAxis, YAxis, CartesianGrid, Line } from 'recharts';
import { RouteComponentProps } from "react-router";

const data = [
    { name: 'A', uv: 4000, pv: 2400, amt: 2400 },
    { name: 'B', uv: 3000, pv: 1398, amt: 2210 },
    { name: 'C', uv: 2000, pv: 9800, amt: 2290 },
    { name: 'D', uv: 2780, pv: 3908, amt: 2000 },
    { name: 'E', uv: 1890, pv: 4800, amt: 2181 },
    { name: 'F', uv: 2390, pv: 3800, amt: 2500 },
    { name: 'G', uv: 3490, pv: 4300, amt: 2100 }
];

export class Statystyki extends React.Component<RouteComponentProps<{}>, {}> {
    public render() {
        return (
            <LineChart width={500} height={300} data={data}>
                <XAxis dataKey="name" />
                <YAxis />
                <CartesianGrid stroke="#eee" strokeDasharray="5 5" />
                <Line type="monotone" dataKey="uv" stroke="#8884d8" />
                <Line type="monotone" dataKey="pv" stroke="#82ca9d" />
            </LineChart>
        );
    }
}