import React, { Component } from 'react';

export class Home extends Component {
    
    constructor (props) {
        super(props);
        this.state = { accessToken: this.props.match.params.accessToken, services: [], loading: true };
        fetch('api/Service/GetServiceData?token=' + this.state.accessToken)
            .then(response => response.json())
            .then(data => {
                let buttonNextClicks = [];
                for (let counter = 0; counter < data.length; counter++) {
                    buttonNextClicks[counter] = false;
                }
                this.setState({bnc : buttonNextClicks, services : data, loading: false});
            });
    }
    
    startService(serviceId) {
        fetch('api/Service/RunService?token=' + this.state.accessToken + '&serviceId=' + serviceId)
            .then(response => response.json())
            .then(data => {
                if (data.status === 'success') {
                    let ser = this.state.services;
                    ser[serviceId].state = 'Running';
                    let buttonNextClicks = [];
                    for (let counter = 0; counter < data.length; counter++) {
                        buttonNextClicks[counter] = false;
                    }
                    this.setState({services: ser, bnc: buttonNextClicks});
                }
            });
    }
    
    stopService(serviceId) {
        fetch('api/Service/StopService?token=' + this.state.accessToken + '&serviceId=' + serviceId)
            .then(response => response.json())
            .then(data => {
                if (data.status === 'success') {
                    let ser = this.state.services;
                    ser[serviceId].state = 'Stopped';
                    let buttonNextClicks = [];
                    for (let counter = 0; counter < data.length; counter++) {
                        buttonNextClicks[counter] = false;
                    }
                    this.setState({services: ser, bnc: buttonNextClicks});
                }
            });
    }
    
    clearDatabase(serviceId) {
        if (window.confirm('Do you really want to delete database ?')) {
            fetch('api/Service/DeleteDatabase?token=' + this.state.accessToken + '&serviceId=' + serviceId)
                .then(response => response.json())
                .then(data => {
                    if (data.status === 'success') {
                        alert('Service database cleared successfully.');
                    }
                });
        }
    }

    renderServicesTable (serviceDataList) {
        return (
            <table className='table table-striped'>
                <thead>
                <tr>
                    <th> ServiceID</th>
                    <th>Service</th>
                    <th>Status</th>
                    <th>LifeCyle Control</th>
                    <th>Version Control</th>
                    <th>Database</th>
                </tr>
                </thead>
                <tbody>
                {serviceDataList.map(serviceData =>
                    <tr key={serviceData.id}>
                        <td>{serviceData.id}</td>
                        <td>{serviceData.name}</td>
                        <td>{serviceData.state}</td>
                        <td>
                            <button onClick={() => {serviceData.state === 'Running' ? 
                                this.stopService(serviceData.id) : this.startService(serviceData.id)}} 
                                    style={{width:'72px', height: '28px'}}>
                                {serviceData.state === 'Running' ? 'Stop' : 'Run'}
                            </button>
                        </td>
                        <td>
                            <form ref={'updateForm' + serviceData.id}
                                  encType={'multipart/form-data'} 
                                  action={'api/Service/UpdateService'} 
                                  method={'post'}>
                                <fieldset disabled={serviceData.state === 'Running'}>
                                    <input id='Files' name={'Files'} type='file' multiple/>
                                    <input id='Token' name={'Token'} type='hidden' value={this.state.accessToken}/>
                                    <input id='ServiceId' name={'ServiceId'} type='hidden' value={serviceData.id}/>
                                    <input id='SubmitButton' name={'Submit'} type='submit' title={'Update'}/>
                                </fieldset>
                            </form>
                        </td>
                        <td>
                            <button onClick={() => this.clearDatabase(serviceData.id)}>
                                Clear Data
                            </button>
                        </td>
                    </tr>
                )}
                </tbody>
            </table>
        );
    }

    render () {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.renderServicesTable(this.state.services);

        return (
            <div>
                <h1>Services Data</h1>
                <p>This is services control panel located on main server</p>
                {contents}
            </div>
        );
    }
}
