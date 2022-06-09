import React, { Component } from 'react';
import { Link } from "react-router-dom";
import { Post } from './Post';
import {variables} from './Variables.js';

export class Home extends Component {
    /* Constructor for the component. Tracks the posts shown on the feed and whether the page's loading status */
    constructor(props) {
        super(props);
        this.state = { 
            posts: [], 
            loading: true
        };
    }

    /* Once the page renders, this lifecycle method takes place */
    componentDidMount(){
        this.getPosts();
    }
    
    /* Makes api call to backend to get all posts */
    async getPosts() {
        const response = await fetch(variables.API_URL+'posts');
        const data = await response.json();
        this.setState({
            posts: data,
            loading: false,
        });
    }

    render() {
        return ( 
            <div>
                <Link to="/">Login</Link>
                <h3> This is the Home page </h3> 
                <div>
                <table>
                    <thead>
                        <tr>
                            <th>Main Feed</th>
                        </tr>
                    </thead>
                    <tbody>
                    {this.state.posts.map(post => <tr><Post postDetails={post} /></tr>)}
                    </tbody>
                </table> 
                </div>
            </div>
        )
    }
}

export default Home;