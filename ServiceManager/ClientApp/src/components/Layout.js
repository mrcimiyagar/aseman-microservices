import React, { Component } from 'react';
import { Container } from 'reactstrap';

export class Layout extends Component {
  static displayName = Layout.name;

  render () {
    return (
        <div>
            <div style={{ marginTop: '100px' }}/>
            <Container>
                {this.props.children}
            </Container>
        </div>
    );
  }
}
